from abc import ABC, abstractmethod


class Intention(ABC):

    @abstractmethod
    def get_labels(self) -> list[str]:
        pass
