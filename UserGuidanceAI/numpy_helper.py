"""
    Create valid typing variables for numpy
"""

import numpy.typing as npt
from typing import Annotated, Literal, TypeVar
import numpy as np

DType = TypeVar("DType", bound=np.generic)

Array4 = Annotated[npt.NDArray[DType], Literal[4]]
Array4x4 = Annotated[npt.NDArray[DType], Literal[4, 4]]
ArrayNx4 = Annotated[npt.NDArray[DType], Literal["N", 4]]
ArrayNxHxW = Annotated[npt.NDArray[DType], Literal["N", "H", "W"]]
