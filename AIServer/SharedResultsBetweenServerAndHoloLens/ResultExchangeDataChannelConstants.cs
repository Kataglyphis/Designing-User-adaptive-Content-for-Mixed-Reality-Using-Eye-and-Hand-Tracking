namespace SharedResultsBetweenServerAndHoloLens
{
    public static class ResultExchangeDataChannelConstants
    {
        public const string DataChannelLabelDetection = "DetectionResults";

        public static string[] DataChannelLabelsSegmentation = {"FirstSegmentationResult",
                                                                "SecondSegmentationResult",
                                                                "ThirdSegmentationResult",
                                                                "FourthSegmentationResult"};

        public static string[] DataChannelLabelsMask = {"FirstMaskResult",
                                                        "SecondMaskResult",
                                                        "ThirdMaskResult",
                                                        "FourthMaskResult"};

        public static string DataChannelUserData = "UserDataChannel";

    }
}
