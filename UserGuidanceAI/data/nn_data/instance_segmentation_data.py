import typing
import torch
import torch.nn.functional as F
from data.nn_data.datasets.dataset import DataSet
from data.nn_data.datasets.truck_dataset import TruckDataSet
from data.nn_data.nn_data import NNData
from numpy_helper import Array4, ArrayNx4, ArrayNxHxW
import numpy as np
import numpy.typing as npt
import constants
import pandas as pd
import base64
import matplotlib.pyplot as plt
import matplotlib.patches as patches

from scipy import interpolate

from sklearn import preprocessing


class InstanceSegmentationData(NNData):

    def __init__(self) -> None:
        NNData.__init__(self, 1)
        self.current_dataset = TruckDataSet()
        self.le = preprocessing.LabelEncoder()
        self.le.fit(self.current_dataset.get_labels())

    def __crop_mask(self, masks: ArrayNxHxW[np.float32], boxes: ArrayNx4[np.float32]) -> torch.Tensor:
        """
        "Crop" predicted masks by zeroing out everything not in the predicted bbox.
        highly inspired by yolov5 implementation:
        https://github.com/ultralytics/yolov5/blob/master/utils/segment/general.py

        Args:
            - masks should be a size [n, h, w] tensor of masks
            - boxes should be a size [n, 4] tensor of bbox coords in relative point form
        """

        n, h, w = masks.shape
        masks_tensor = torch.tensor(masks, dtype=torch.float32)
        boxes_tensor = torch.tensor(boxes, dtype=torch.float32)
        y1, x1, y2, x2 = torch.chunk(boxes_tensor[:, :, None], 4, 1)  # x1 shape(1,1,n)
        r = torch.arange(w, device=masks_tensor.device, dtype=x1.dtype)[None, None, :]  # rows shape(1,w,1)
        c = torch.arange(h, device=masks_tensor.device, dtype=x1.dtype)[None, :, None]  # cols shape(h,1,1)

        return masks_tensor * ((r >= x1) * (r < x2) * (c >= y1) * (c < y2))

    def __get_mask_columns_to_join(self) -> list[list[str]]:
        """
        Get the columns that are used to store the masks
        """
        mask_columns_to_join = []
        for i in range(len(self.Labels)//self.get_entry_size()):
            mask_columns_to_join.append([f"{self.Labels[i*self.get_entry_size() + constants.MASK_PART1_INDEX]}", 
                                         f"{self.Labels[i*self.get_entry_size() + constants.MASK_PART2_INDEX]}"])
        return mask_columns_to_join

    def get_joined_mask_columns(self) -> list[str]:
        """
        Get the columns that are used to store the masks
        """
        mask_columns_joined = []
        for i in range(constants.MAX_DETECTION_RESULTS):
            mask_columns_joined.append(f"segmentation mask {i}")
        return mask_columns_joined

    def encode_class_label(self, class_label: str) -> int:
        return self.le.transform([class_label])[0]

    def recreate_masks_in_pandas_df(self, pandas_data_frame: pd.DataFrame) -> np.array:
        """
        Recreate the masks in one pandas dataframe;
        We stored the masks as base64 encoded strings in the csv file in two parts
        """
        mask_columns_to_join = self.__get_mask_columns_to_join()
        mask_columns_joined = self.get_joined_mask_columns()

        window_shape = (constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y,
                        constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_X)
        
        mask_shape = (constants.YOLOV5_MASK_SIZE_X,
                      constants.YOLOV5_MASK_SIZE_Y)
        
        returned_masks = np.zeros((len(mask_columns_joined),
                                   pandas_data_frame.shape[0],
                                   mask_shape[0],
                                   mask_shape[1]))

        for index, value in np.ndenumerate(mask_columns_joined):

            # bring index into correct shape
            index = index[0]
            # aux_value = value + f" {index} uncropped"
            # bring all mask values into one column
            pandas_data_frame[value] = (pandas_data_frame[mask_columns_to_join[index][0]].astype(str) +
                                        pandas_data_frame[mask_columns_to_join[index][1]].astype(str))

            # only restore values that really represent an image mask
            # we concatenated two columns therefore the "nannan"
            # mask_NaN_valuesOLD = (pandas_data_frame[value] == "nannan")
            mask_NaN_values = np.equal(pandas_data_frame[value], "nannan")

            pandas_data_frame.loc[mask_NaN_values, value] = "NaN"
            mask_non_NaN_values = (pandas_data_frame[value] != "NaN")
            # mask_non_NaN_values = np.not_equal(pandas_data_frame[value], "NaN")

            # pandas_data_frame.loc[mask_NaN_values,value] = np.zeros(shape=(mask_shape[1] * mask_shape[0]),
            #                                                         dtype=np.float32),
            
            # pandas_data_frame[value].replace(to_replace=['NaN'],
            #                                  value=np.zeros(shape=(mask_shape[1] * mask_shape[0]),
            #                                                        dtype=np.float32),
            #                                  #  method='ffill',
            #                                  inplace=True)
            # pandas_data_frame[value].replace(to_replace=['NaN'], method='bfill', inplace=True)
            returned_masks[index] = np.zeros(shape=(len(pandas_data_frame.index), mask_shape[0], mask_shape[1]),
                                             dtype=np.float32)
            # on server side we encoded our results as base64 strings
            # do this only if we have any masks at all
            if not pandas_data_frame[value].isin(["NaN"]).all():

                pandas_data_frame.loc[mask_non_NaN_values, value] = pandas_data_frame.loc[mask_non_NaN_values, value].apply(base64.b64decode)
                pandas_data_frame.loc[mask_non_NaN_values, value] = pandas_data_frame.loc[mask_non_NaN_values, value].apply(list)
                pandas_data_frame.loc[mask_non_NaN_values, value] = pandas_data_frame.loc[mask_non_NaN_values, value].apply(np.array)
                pandas_data_frame.loc[mask_non_NaN_values, value] = pandas_data_frame.loc[mask_non_NaN_values, value].apply(lambda x: np.reshape(x,
                                                                                                                            newshape=(
                                                                                                                            mask_shape[1],
                                                                                                                            mask_shape[0])))

                bbox_headings_instance_segmentation = self.get_bboox_labels()
                # pandas_data_frame[bbox_headings_instance_segmentation[index]].replace(to_replace=np.nan, method='ffill', inplace=True)
                # pandas_data_frame[bbox_headings_instance_segmentation[index]].replace(to_replace=np.nan, method='bfill', inplace=True)

                # masks
                returned_masks[index, mask_non_NaN_values] = np.stack(pandas_data_frame.loc[mask_non_NaN_values, value].to_numpy())
                boxes = np.stack(pandas_data_frame[bbox_headings_instance_segmentation[index]].to_numpy())
                # bbox layout is as follows:
                # bbox[0]: top, bbox[0]: left, bbox[0]: bottom, bbox[0]: right
                boxes[:, 0] *= constants.YOLOV5_MASK_SIZE_Y / constants.YOLOV5_BBOX_SIZE_Y
                boxes[:, 2] *= constants.YOLOV5_MASK_SIZE_Y / constants.YOLOV5_BBOX_SIZE_Y
                boxes[:, 1] *= constants.YOLOV5_MASK_SIZE_X / constants.YOLOV5_BBOX_SIZE_X
                boxes[:, 3] *= constants.YOLOV5_MASK_SIZE_X / constants.YOLOV5_BBOX_SIZE_X
                # scale boxes appropriately to the size of the mask

                returned_masks[index] = (self.__crop_mask(returned_masks[index], boxes)).numpy()
                returned_masks[index][returned_masks[index] > 0.5] = 1
                # masks

            # else:

            #     returned_masks[index] = np.zeros(shape=(len(pandas_data_frame.index), mask_shape[0], mask_shape[1]),
            #                                      dtype=np.float32)

        return returned_masks

    def write_detection_results(self,
                                df_tracking_data_old: pd.DataFrame,
                                df_tracking_data_preprocessed: pd.DataFrame) -> None:

        class_labels = self.get_class_labels()
        bbox_labels = self.get_bboox_labels()
        df_tracking_data_preprocessed[class_labels] = df_tracking_data_old[class_labels]

        # for index, value in enumerate(bbox_labels):
        #     bbox_arr = df_tracking_data_old.loc[:, value]
        #     df_tracking_data_preprocessed[value + " x_coord"] = bbox_arr[0][0]
        #     df_tracking_data_preprocessed[value + " y_coord"] = bbox_arr[0][1]
        #     df_tracking_data_preprocessed[value + " x_extent"] = bbox_arr[0][2]
        #     df_tracking_data_preprocessed[value + " y_extent"] = bbox_arr[0][3]

        # df_tracking_data_preprocessed[bbox_labels] = df_tracking_data_old[bbox_labels]

    def get_dtypes(self) -> list[npt.DTypeLike]:
        """
        Get the dtypes
        Returns:
            list[npt.DTypeLike]: the dtypes
        """
        aux = np.tile(np.array([str, np.dtype(object), np.float32, str, str]), constants.MAX_DETECTION_RESULTS)
        return aux.tolist()

    def get_entry_size(self) -> int:
        """
        Get the size of the data entry
        Returns:
            int: the size of the data entry
        """
        return 5

