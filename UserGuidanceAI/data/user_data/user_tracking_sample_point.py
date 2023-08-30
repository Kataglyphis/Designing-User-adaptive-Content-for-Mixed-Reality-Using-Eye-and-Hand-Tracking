from typing import Any


class UserTrackingSamplePoint:

    def __init__(self,
                 LabelsVectorizedDescriptionsDescriptionDict: dict[str, str]
                 ) -> None:
        self.__dict__ = LabelsVectorizedDescriptionsDescriptionDict

    Labels = list[str]
    VectorizedDescription = list[str]
    Description = list[str]
