from data.nn_data.datasets.dataset import DataSet


class TruckDataSet(DataSet):

    def get_labels(self) -> list[str]:
        return ["", "cap", "frame", "platform"]
