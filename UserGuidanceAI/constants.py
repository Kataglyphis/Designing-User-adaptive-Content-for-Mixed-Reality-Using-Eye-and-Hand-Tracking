# define constants
DATA_FOLDER = "data/collected_data/"
TRAIN_DATA_FOLDER = DATA_FOLDER + "train/"
TEST_DATA_FOLDER = DATA_FOLDER + "test/"
VALIDATION_DATA_FOLDER = DATA_FOLDER + "validation/"
CSV_FILE_DELIMITER = ";"

# all related constants we desperately need for working with YOLOv5 results
YOLOV5_MASK_SIZE_X = 160
YOLOV5_MASK_SIZE_Y = 160
YOLOV5_BBOX_SIZE_X = 640
YOLOV5_BBOX_SIZE_Y = 640

COLOR_THEME = "#69F0AE"

# maximum supported render target sizes hololens2
# src: https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/rendering-overview
MAX_RENDER_TARGET_SIZE_HOLOLENS2_X = 1440
MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y = 936

# basic data constants
INTENTION_INDEX = 1

# constants for eye data
EYE_HIT_POSITION_INDEX = 7

# constants for the nn data layout
MAX_DETECTION_RESULTS = 4
CLASS_LABEL_INDEX = 0
BBOX_INDEX = 1
CLASS_PROB_INDEX = 2
MASK_PART1_INDEX = 3
MASK_PART2_INDEX = 4
# object detection constants
OBJECT_DETECTION_INDEX = 0
# instance segmentation constants
INSTANCE_SEGMENTATION_INDEX = 1
INSTANCE_SEGMENTATION_RESULT_SIZE = 5

TRANSFORM_DATA_INDEX = 0
USER_DATA_INDEX = 1
EYE_DATA_INDEX = 0
HAND_DATA_INDEX = 1
HEAD_DATA_INDEX = 2

VISUALIZER_IMAGE_PATH = "data/visualizer_images/"
MODELS_DATA_PATH = "models/"
# Size in sample points
SLIDING_WINDOW_SIZE = 50
