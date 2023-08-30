from typing import Any
from data.user_data.user_tracking_sample_point import UserTrackingSamplePoint
from data.user_data.user_data_cluster import UserDataCluster
import numpy.typing as npt
import numpy as np
import constants
import pandas as pd


class HeadData(UserDataCluster):

    def __init__(self) -> None:
        UserDataCluster.__init__(self, constants.HEAD_DATA_INDEX)

    def get_dtypes(self) -> list[npt.DTypeLike]:
        """
        Get the dtypes
        Returns:
            list[npt.DTypeLike]: the dtypes
        """
        return [np.float32 for x in range(len(self.Labels))]

    def get_new_direction_labels(self) -> list[list[str]]:
        """
        Get the new eye hit position label
        """
        return [["HeadMovement.x",
                 "HeadMovement.y",
                 "HeadMovement.z"],
                ["HeadVelocity.x",
                 "HeadVelocity.y",
                 "HeadVelocity.z"]]

    def transfer_data(self, df_tracking_data: pd.DataFrame, df_tracking_data_preprocessed: pd.DataFrame) -> None:
        """
        Transfer the head data to the preprocessed dataframe
        """
        position_labels = self.get_new_direction_labels()
        indices = [9, 12]

        for index, position_label in enumerate(position_labels):

            # all data is 3d
            labels = [self.Labels[indices[index]],
                      self.Labels[indices[index] + 1],
                      self.Labels[indices[index] + 2]]

            head_data = df_tracking_data[labels].to_numpy()

            df_tracking_data_preprocessed[position_label[0]] = head_data[:,0]
            df_tracking_data_preprocessed[position_label[1]] = head_data[:,1]
            df_tracking_data_preprocessed[position_label[2]] = head_data[:,2]

    def create_positions_screen_space(self,
                                      mvp_matrices: npt.ArrayLike,
                                      old_df: pd.DataFrame,
                                      new_df: pd.DataFrame) -> None:
        pass

    def get_new_position_labels(self) -> list[list[str]]:
        return [[]]
