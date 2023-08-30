from abc import ABC, abstractmethod
import numpy as np
import torch
import json
import constants
from data.data_cluster_meta_information import DataClusterMetaInformation
from numpy_helper import Array4, ArrayNx4, ArrayNxHxW


class NNData(DataClusterMetaInformation, ABC):

    def __init__(self, index: int) -> None:
        f = open('data/nn_data/NNData.json')
        nn_data: list[dict[str, str]] = json.load(f)
        self.dataDescriptionLabelsDict: dict[str, str] = nn_data[index]
        DataClusterMetaInformation.__init__(self, self.dataDescriptionLabelsDict)

    @abstractmethod
    def get_entry_size(self) -> int:
        """
        Get the size of the data entry
        Returns:
            int: the size of the data entry
        """
        pass

    def __get_entries(self, index: int) -> list[str]:
        """
        Get all column labels of the corresponding bounding boxes
        Returns:
            list[str]: the restored labels
        """
        return [self.Labels[x * self.get_entry_size() + index]
                for x in range(len(self.Labels) // self.get_entry_size())]

    def get_class_labels(self) -> list[str]:
        """
        Get all column labels of the corresponding bounding boxes
        Returns:
            list[str]: the restored labels
        """
        return self.__get_entries(constants.CLASS_LABEL_INDEX)

    def get_prob_labels(self) -> list[str]:
        """
        Get all column labels of the corresponding bounding boxes
        Returns:
            list[str]: the restored labels
        """
        return self.__get_entries(constants.CLASS_PROB_INDEX) 

    def get_bboox_labels(self) -> list[str]:
        """
        Get all column labels of the corresponding bounding boxes
        Returns:
            list[str]: the restored labels
        """
        return self.__get_entries(constants.BBOX_INDEX)

    def recreate_bboxes(self, bbox: str) -> Array4[np.float32]:
        """
        We store the bbox as a single string of continuous floats
        Here recreate the bbox
        Parameters:
            bbox (string): The string we read from the csv file
        Returns:
            np.array: the restored bbox
                    index | content
                        0   |   top
                        1   |   left
                        2   |   bottom
                        3   |   right
        """
        aux = bbox.split()
        return ([float(aux[0]), float(aux[1]), float(aux[2]), float(aux[3])] if (len(bbox) != 0)
                else [0, 0, 0, 0])  # np.nannp.zeros(shape=(4), dtype=np.float32))"NaN"
        # return (np.array(bbox.split(), dtype=np.float32).reshape(4) if (len(bbox) != 0)
        #         else (np.zeros(4, dtype=np.float32)))  # np.nannp.zeros(shape=(4), dtype=np.float32))"NaN"

    def recreate_class_probs(self, prob: str) -> str:
        """
        We store the prob as a single string
        Parameters:
            prob (string): The string we read from the csv file
        Returns:
            double in European style
        """
        return prob.replace('.', ',')
