from data.basic_data.intentions import Intention


class TruckDiscoverIntention(Intention):

    def get_labels(self) -> list[str]:
        return ["NoIntention", "FirstIntention", "SecondIntention", "ThirdIntention"]