from abc import ABC, abstractmethod
import numpy as np
import numpy.typing as npt


class DataClusterMetaInformation(ABC):

    def __init__(self, dataDescriptionLabelsDict: dict[str, str]) -> None:
        self.Data: list[str]
        self.Description: list[str]
        self.Labels: list[str]
        self.__dict__ = dataDescriptionLabelsDict

    @abstractmethod
    def get_dtypes(self) -> list[npt.DTypeLike]:
        """
        Get the dtypes
        Returns:
            list[npt.DTypeLike]: the dtypes
        """
        pass

    def get_columns_dtypes(self) -> dict[str, npt.DTypeLike]:
        """
        Get the column dtypes
        Returns:
            dict[str, npt.]: the column dtypes
        """
        dtypes = self.get_dtypes()
        assert len(dtypes) == len(self.Labels)
        return {self.Labels[i]: dtypes[i]
                for i in range(len(self.Labels))}
