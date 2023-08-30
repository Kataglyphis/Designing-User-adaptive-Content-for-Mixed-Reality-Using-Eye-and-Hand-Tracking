import typing
from data.nn_data.nn_data import NNData
import numpy as np
import numpy.typing as npt
import constants
from numpy_helper import Array4


class ObjectDetectionData(NNData):

    def __init__(self) -> None:
        NNData.__init__(self, 0)

    def get_dtypes(self) -> list[typing.Union[npt.NDArray, str]]:
        """
        Get the dtypes
        Returns:
            list[npt.DTypeLike]: the dtypes
        """
        aux = np.tile(np.array([str, np.dtype(object), np.float32]), constants.MAX_DETECTION_RESULTS)
        return aux.tolist()  

    def get_entry_size(self) -> int:
        """
        Get the size of the data entry
        Returns:
            int: the size of the data entry
        """
        return 3
