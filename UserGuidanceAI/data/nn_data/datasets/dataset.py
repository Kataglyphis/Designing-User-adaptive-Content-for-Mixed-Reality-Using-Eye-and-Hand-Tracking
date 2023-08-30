from abc import ABC, abstractmethod


class DataSet(ABC):

    @abstractmethod
    def get_labels(self) -> list[str]:
        pass
