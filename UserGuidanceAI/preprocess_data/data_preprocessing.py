import os
import pandas as pd
import numpy as np
import numpy.typing as npt

import constants

from data.basic_data.basic_data import BasicData
from data.nn_data.instance_segmentation_data import InstanceSegmentationData
from data.nn_data.object_detection_data import ObjectDetectionData
from data.user_data.eye_data import EyeData
from data.user_data.hand_data import HandData
from data.user_data.head_data import HeadData
from data.user_data.transform_data import TransformData

from visualization.visualizer import Visualizer


class DataPreprocessing():

    def __init__(self) -> None:
        pass

    @staticmethod
    def get_all_folder_names_of_dir(dir: str) -> list[str]:
        '''
        Retrieve all folder names of a given directory
        '''
        folder_names = []
        for folder_name in os.listdir(dir):
            entry_path = os.path.join(dir, folder_name)
            if os.path.isdir(entry_path):
                folder_names.append(folder_name)
        return folder_names

    @staticmethod
    def get_all_file_names_of_dir(dir: str) -> list[str]:
        '''
        Retrieve all file names of a given directory
        '''
        file_names = []
        for file_name in os.listdir(dir):
            file_path = os.path.join(dir, file_name)
            if os.path.isfile(file_path):
                file_names.append(file_name)
        return file_names

    @staticmethod
    def preprocess_user_data(data_folder: str) -> pd.DataFrame:
        '''
        Preprocess the user data and save the results in a new csv file
        '''
        user_names = DataPreprocessing.get_all_folder_names_of_dir(data_folder)
        user_data = []

        # num_users = 0
        vis = Visualizer()
        head_data = HeadData()

        for user_name in user_names:
            if user_name != "Jonas":
                continue
            # if num_users == 1:
            #     break
            # num_users += 1
            user_dir = data_folder + user_name
            intentions = DataPreprocessing.get_all_folder_names_of_dir(user_dir)
            for intention in intentions:

                if intention != "SecondIntention":
                    continue

                intention_dir = user_dir + "/" + intention
                csv_file_names = DataPreprocessing.get_all_file_names_of_dir(intention_dir)
                num_csv_files = 0
                for csv_file_name in csv_file_names:
                    num_csv_files += 1
                    csv_file = intention_dir + "/" + csv_file_name
                    user_data.append(DataPreprocessing.preprocess_csv_file(csv_file,
                                                                           user_name,
                                                                           intention))
                    # if num_csv_files == 1:
                    #     break

        head_labels = head_data.get_new_direction_labels()

        vis.visualize_head_data(pd.concat(user_data),
                                head_labels)

        return pd.concat(user_data)

    @staticmethod
    def preprocess_csv_file(csv_file: str,
                            user_name: str,
                            intention: str) -> pd.DataFrame:
        '''
        Preprocess the user data and save the results in a new csv file
        '''
        # load the data from the csv file
        # we have to specify the data types of the columns
        # we have to specify the converters for the columns which contain matrices
        # we have to specify the converters for the columns which contain bounding boxes
        # we have to specify the converters for the columns which contain class probabilities
        # we have to skip the first 5 seconds of the recording

        # in the next steps we load the data layout from our recorded data
        # this is important for accessing the data in the correct way when
        # we load the corresponding data from the csv file
        basicData = BasicData()
        # all neural net related stuff
        nnDataObjectDetection = ObjectDetectionData()
        nnDataInstanceSegmentation = InstanceSegmentationData()
        # all data collected from user
        transformData = TransformData()
        # big list of all user data points
        eye_data = EyeData()
        hand_data = HandData()
        head_data = HeadData()

        new_dtypes = (eye_data.get_columns_dtypes() |
                      hand_data.get_columns_dtypes() |
                      head_data.get_columns_dtypes())

        converters = ({k: transformData.recreate_matrix for k in transformData.Labels} |
                      {k: nnDataObjectDetection.recreate_bboxes
                      for k in nnDataObjectDetection.get_bboox_labels()} |
                      {k: nnDataInstanceSegmentation.recreate_bboxes
                      for k in nnDataInstanceSegmentation.get_bboox_labels()} |
                      {k: nnDataInstanceSegmentation.recreate_class_probs
                      for k in nnDataInstanceSegmentation.get_prob_labels()} |
                      {k: nnDataInstanceSegmentation.encode_class_label
                      for k in nnDataInstanceSegmentation.get_class_labels()} |
                      {k: basicData.create_class_label
                      for k in basicData.get_intention_label()})

        # index of row to use as header
        header_index = 0
        # how many rows should we skip in the beginning?
        rows_to_skip_in_the_beginning = 2

        # all our tracked data from one session
        # needed preprocessing is already done while loading
        df_tracking_data = pd.read_csv(csv_file,
                                       header=header_index,
                                       delimiter=constants.CSV_FILE_DELIMITER,
                                       dtype=new_dtypes,
                                       # 165we have one description row[1],
                                       # skip the first 5 seconds (5x 33(SAMPLES/Second) = 165
                                       skiprows=lambda x: x in range(1, rows_to_skip_in_the_beginning),
                                       decimal=",",  # we work with European data
                                       skipinitialspace=True,
                                       index_col=False,
                                       converters=converters
                                       )

        # this data frame will only contain all data we want for training
        # and all data will be in correct format
        df_tracking_data_preprocessed = pd.DataFrame()

        basicData.write_results(df_tracking_data,
                                df_tracking_data_preprocessed)

        # store the user data as 2d screen space data
        mvp_matrices = transformData.get_mvp_matrices(df_tracking_data)
        eye_data.create_positions_screen_space(mvp_matrices, df_tracking_data, df_tracking_data_preprocessed)
        hand_data.create_positions_screen_space(mvp_matrices, df_tracking_data, df_tracking_data_preprocessed)
        head_data.transfer_data(df_tracking_data, df_tracking_data_preprocessed)

        nnDataInstanceSegmentation.write_detection_results(df_tracking_data,
                                                           df_tracking_data_preprocessed)

        new_screen_space_eye_hit_position_label = eye_data.get_new_position_labels()
        new_screen_space_hand_position_label = hand_data.get_new_position_labels()

        visualizer = Visualizer()

        visualizer.visualize_all_2d_positions_screen_space(df_tracking_data_preprocessed,
                                                           new_screen_space_eye_hit_position_label + new_screen_space_hand_position_label,
                                                           user_name,
                                                           intention,
                                                           visualize_bursts=True,
                                                           step_size=10,
                                                           offset=30,
                                                           colors=['b', 'g', 'r'],
                                                           burst_size=160)

        # masks = nnDataInstanceSegmentation.recreate_masks_in_pandas_df(df_tracking_data)

        # in_or_out_labels_eye_hit_pos = DataPreprocessing.check_if_2d_positions_are_in_mask(df_tracking_data_preprocessed,
        #                                                                                    new_screen_space_eye_hit_position_label,
        #                                                                                    masks)

        # in_or_out_labels_hand_pos = DataPreprocessing.check_if_2d_positions_are_in_mask(df_tracking_data_preprocessed,
        #                                                                                 new_screen_space_hand_position_label,
        #                                                                                 masks)

        df_tracking_data_preprocessed.dropna(inplace=True)

        # visualizer.visualize_results(df_tracking_data,
        #                              df_tracking_data_preprocessed,
        #                              eye_data,
        #                              in_or_out_labels_eye_hit_pos,
        #                              hand_data,
        #                              in_or_out_labels_hand_pos,
        #                              head_data,
        #                              nnDataInstanceSegmentation,
        #                              masks,
        #                              user_name,
        #                              intention)

        return df_tracking_data_preprocessed

    @staticmethod
    # transform a time series dataset into a supervised learning dataset
    def series_to_supervised(eye_tracking_data: pd.DataFrame,
                             n_in: int = constants.SLIDING_WINDOW_SIZE,
                             n_out: int = 0,
                             dropnan: bool = True) -> pd.DataFrame:
        '''
        Preprocess the user data and save the results in a new csv file
        '''
        agg = eye_tracking_data.iloc[:, [0]].copy()

        # input sequence (t-n, ... t-1)
        for column_index, column in enumerate(eye_tracking_data.iloc[:, 1:], start=1):
            # cols = list()
            for i in range(n_in - 1, 0, -1):
                shifted_column = eye_tracking_data.iloc[:, column_index].shift(i)
                shifted_column.rename(column + f" t_{i}", inplace=True)
                agg = pd.concat([agg, shifted_column], axis=1)
            agg = pd.concat([agg, eye_tracking_data[column]], axis=1)

        if dropnan:
            agg.dropna(inplace=True)

        return agg  # .values

    @staticmethod
    def check_if_2d_positions_are_in_mask(data_frame: pd.DataFrame,
                                          new_screen_space_position_labels: list[list[str]],
                                          masks: npt.ArrayLike) -> list[str]:
        '''
        Check if the 2d positions are in the mask and store boolean value accordingly
        '''
        in_or_out_labels = []

        for new_screen_space_position_label in new_screen_space_position_labels:
            restored_clip_space_pos = data_frame[new_screen_space_position_label].to_numpy()
            restored_clip_space_pos = np.clip(restored_clip_space_pos, -1, 1)

            window_shape = (constants.YOLOV5_MASK_SIZE_X - 1,
                            constants.YOLOV5_MASK_SIZE_Y - 1)

            rescaled_clip_space_pos = ((restored_clip_space_pos + 1.0) / 2.0) * window_shape
            rescaled_clip_space_pos = rescaled_clip_space_pos.astype(np.uint16)

            in_or_out = np.empty((masks.shape[0], masks.shape[1]))

            for segmentation_masks in range(masks.shape[0]):
                masks_results = np.empty(masks.shape[1])
                for samples in range(masks.shape[1]):
                    masks_results[samples] = masks[segmentation_masks, samples, rescaled_clip_space_pos[samples, 1], rescaled_clip_space_pos[samples ,0]]  #, rescaled_clip_space_pos[:,0]
                in_or_out[segmentation_masks] = masks_results

            in_or_out_labels_single_pos = []
            for i in range(masks.shape[0]):
                in_or_out_labels_single_pos.append(f"{i}.segmentation mask " + new_screen_space_position_label[0].split('.')[0] + " in or out")
            in_or_out_labels.append(in_or_out_labels_single_pos)

            data_frame[in_or_out_labels_single_pos] = in_or_out.T
        
        return in_or_out_labels
