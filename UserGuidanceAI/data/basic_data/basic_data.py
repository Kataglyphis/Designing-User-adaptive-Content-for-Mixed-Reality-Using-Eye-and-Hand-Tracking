import numpy as np
import numpy.typing as npt
import pandas as pd
import json

from sklearn import preprocessing
from data.basic_data.truck_discover_intentions import TruckDiscoverIntention

from data.data_cluster_meta_information import DataClusterMetaInformation
import constants


class BasicData(DataClusterMetaInformation):

    def __init__(self) -> None:
        f = open('data/basic_data/BasicData.json')
        self.dataDescriptionLabelsDict: dict[str, str] = json.load(f)
        DataClusterMetaInformation.__init__(self, self.dataDescriptionLabelsDict)
        self.current_intentions = TruckDiscoverIntention()
        self.le = preprocessing.LabelEncoder()
        self.le.fit(self.current_intentions.get_labels())

    def get_dtypes(self) -> list[npt.DTypeLike]:
        """
        Get the dtypes
        Returns:
            list[npt.DTypeLike]: the dtypes
        """
        return [np.dtype(np.str_), np.dtype(np.str_), np.dtype(np.float32)]

    def get_intention_label(self) -> list[str]:
        """
        Get the intention labels
        Returns:
            list[str]: the intention labels
        """
        return [self.Labels[constants.INTENTION_INDEX]]

    def create_class_label(self, intention_string: str) -> int:
        """
        Create a corressponding class label to an intention string
        """
        return self.le.transform([intention_string])[0]

    def write_results(self,
                      df_tracking_data_old: pd,
                      df_tracking_data_new: pd) -> None:
        """
        Copy the intention label from the old dataframe to the new dataframe
        """
        df_tracking_data_new[self.get_intention_label()] = df_tracking_data_old[self.get_intention_label()]
