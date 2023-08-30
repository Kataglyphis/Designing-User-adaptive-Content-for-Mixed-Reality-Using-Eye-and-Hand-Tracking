from typing import Any

import numpy as np
import numpy.typing as npt
import pandas as pd

from data.data_cluster_meta_information import DataClusterMetaInformation
from numpy_helper import Array4x4
import constants
import json


class TransformData(DataClusterMetaInformation):

    def __init__(self) -> None:
        f = open('data/user_data/UserData.json')
        self.dataDescriptionLabelsDict: dict[str, str] = json.load(f)[constants.TRANSFORM_DATA_INDEX]
        DataClusterMetaInformation.__init__(self, self.dataDescriptionLabelsDict)

    def get_dtypes(self) -> list[npt.DTypeLike]:
        """
        Get the dtypes
        Returns:
            list[npt.DTypeLike]: the dtypes
        """
        return [np.float32 for x in range(len(self.Labels))]

    def recreate_matrix(self, matrix: str) -> Array4x4[np.float32]:
        """
        We store the 4x4 matrix as a single string of continuous floats
        Here recreate the matrix
        Parameters:
            matrix (string): The string we read from the csv file
        Returns:
            np.array: the restored 4x4 matrix
        """
        return np.array(matrix.split(), dtype=np.float32).reshape(4, 4)

    def get_mvp_matrices(self, df_tracking_data: pd.DataFrame) -> np.array:
        
        mvp_column_names = self.Labels
        view_matrix = np.stack(df_tracking_data[mvp_column_names[0]].to_numpy(), axis=0)
        projection_matrix = np.stack(df_tracking_data[mvp_column_names[1]].to_numpy(), axis=0)

        return projection_matrix @ view_matrix

