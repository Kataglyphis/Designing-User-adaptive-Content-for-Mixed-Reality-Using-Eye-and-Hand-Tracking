from typing import Any
from data.data_cluster_meta_information import DataClusterMetaInformation
from data.user_data.user_tracking_sample_point import UserTrackingSamplePoint
from abc import ABC, abstractmethod
import json
import constants
import numpy.typing as npt
import pandas as pd


class UserDataCluster(DataClusterMetaInformation, ABC):

    def __init__(self,
                 body_part_index: int,
                 ) -> None:
        f = open('data/user_data/UserData.json')
        user_data: list[dict[str, str]] = json.load(f)[constants.USER_DATA_INDEX:]
        body_part_data = user_data[body_part_index]
        # TODO: CHECK THIS
        self.dataDescriptionLabelsDict: dict[str, str] = dict(list(body_part_data.items())[1:])
        self.userTrackingSamplePoints: dict[str, str] = dict(list(body_part_data.items())[:1])
        DataClusterMetaInformation.__init__(self, self.dataDescriptionLabelsDict)

    @abstractmethod
    def get_new_position_labels(self) -> list[list[str]]:
        pass

    @abstractmethod
    def get_new_direction_labels(self) -> list[list[str]]:
        pass

    @abstractmethod
    def create_positions_screen_space(self,
                                      mvp_matrices: npt.ArrayLike,
                                      old_df: pd.DataFrame,
                                      new_df: pd.DataFrame) -> None:
        pass
