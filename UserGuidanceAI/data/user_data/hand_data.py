from typing import Any
import numpy.typing as npt
import numpy as np
from data.user_data.user_tracking_sample_point import UserTrackingSamplePoint
from data.user_data.user_data_cluster import UserDataCluster
import constants
import pandas as pd
from math_helper.math_helper import MathHelper


class HandData(UserDataCluster):

    def __init__(self) -> None:
        UserDataCluster.__init__(self, constants.HAND_DATA_INDEX)

    def get_dtypes(self) -> list[npt.DTypeLike]:
        """
        Get the dtypes
        Returns:
            list[npt.DTypeLike]: the dtypes
        """
        return [np.float32 for x in range(len(self.Labels))]

    def get_new_direction_labels(self) -> list[list[str]]:
        return [[]]

    def get_new_position_labels(self) -> list[list[str]]:
        """
        Get the new eye hit position label
        """
        return [["RightIndexTipPosition (screen space).x",
                "RightIndexTipPosition (screen space).y"],
                ["LeftIndexTipPosition (screen space).x",
                "LeftIndexTipPosition (screen space).y"]]

    def create_positions_screen_space(self,
                                      mvp_matrices: npt.ArrayLike,
                                      old_df: pd.DataFrame,
                                      new_df: pd.DataFrame) -> None:
        """
        Transform the eye hit positions to screen space and add them to the new dataframe
        """
        position_labels = self.get_new_position_labels()
        indices = [26, 39]

        for index, position_label in enumerate(position_labels):

            # all data is 3d
            eye_hit_pos_labels = [self.Labels[indices[index]],
                                  self.Labels[indices[index] + 1],
                                  self.Labels[indices[index] + 2]]

            eye_hit_pos_world_space = old_df[eye_hit_pos_labels].to_numpy()

            eye_hit_pos_screen_space_2d = MathHelper.transform_to_2d_screen_space_positions(mvp_matrices,
                                                                                            eye_hit_pos_world_space)

            new_df[position_label[0]] = eye_hit_pos_screen_space_2d[:, 0]
            new_df[position_label[1]] = eye_hit_pos_screen_space_2d[:, 1]
