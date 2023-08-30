"""Collection of math functions
"""
import doctest

import numpy as np
import numpy.typing as npt


class MathHelper:

    def __init__(self) -> None:
        pass

    @staticmethod
    def clamp(num: float, min_value: float, max_value: float) -> float:
        """
        The function for clamp a value between two values
        Parameters:
            min_value (float): The lower bound
            max_value (float): The upper bound
        Returns:
            float: the clamped value
        >>> clamp(5.0, 0.0, 10.0)
        5.0
        >>> clamp(10.0, 12.0, 120.0)
        12.0
        >>> clamp(-5.0, -2.0, 22.0)
        -2.0
        >>> clamp("hi", 2.0, 10.0)
        Traceback (most recent call last):
        ...
        TypeError: '<' not supported between instances of 'float' and 'str'
        """
        return max(min(num, max_value), min_value)

    @staticmethod
    def transform_to_2d_screen_space_positions(mvp_matrices: npt.ArrayLike,
                                               world_positions: npt.ArrayLike,) -> npt.ArrayLike:
        """
        Transform the 3d  world positions to 2d screen space positions
        """
        world_positions_4d = np.insert(world_positions, 3, np.ones(world_positions.shape[0]), 1)
        screen_space_position = mvp_matrices @ world_positions_4d[:, :, np.newaxis]
        screen_space_position_2d = screen_space_position[:, :2, 0]
        screen_space_position_2d = np.clip(screen_space_position_2d, -1, 1)
        return screen_space_position_2d
    
    @staticmethod
    def screen_space_to_ndc(screen_space_pos: npt.ArrayLike,
                            shape: npt.ArrayLike) -> npt.ArrayLike:
        '''
        Receiving corrds in [-1,1]; return to [0, width] and [0, height]
        '''
        # transform to [0,1]
        return ((screen_space_pos + 1.0) / 2.0) * shape



if __name__ == "__main__":
    doctest.testmod()
