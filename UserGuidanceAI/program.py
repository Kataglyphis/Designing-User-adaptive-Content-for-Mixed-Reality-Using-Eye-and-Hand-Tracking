import pandas as pd

import torch

import numpy as np
import numpy.typing as npt

import constants
from data.basic_data.truck_discover_intentions import TruckDiscoverIntention
from data.nn_data.datasets.truck_dataset import TruckDataSet
from data.nn_data.instance_segmentation_data import InstanceSegmentationData
from data.user_data.eye_data import EyeData
from data.user_data.hand_data import HandData
from data.user_data.head_data import HeadData
import math_helper.math_helper as math_helper

from numpy_helper import Array4, Array4x4, ArrayNx4, ArrayNxHxW

import seaborn as sns
import re

from skl2onnx import convert_sklearn, to_onnx
from skl2onnx.common.data_types import FloatTensorType
from skl2onnx import __max_supported_opset__

from sklearn import tree
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.model_selection import GridSearchCV
from sklearn.ensemble import RandomForestClassifier
from sklearn.preprocessing import StandardScaler
from sklearn.pipeline import make_pipeline, Pipeline
from sklearn.metrics import accuracy_score, confusion_matrix, precision_score, recall_score, ConfusionMatrixDisplay, RocCurveDisplay, f1_score, make_scorer, balanced_accuracy_score
from sklearn.compose import ColumnTransformer
from sklearn.impute import SimpleImputer
from sklearn.model_selection import RandomizedSearchCV, train_test_split
from sklearn.preprocessing import LabelBinarizer, StandardScaler, OneHotEncoder
from sklearn.neural_network import MLPClassifier

from scipy.stats import randint
from preprocess_data.data_preprocessing import DataPreprocessing
from visualization.visualizer import Visualizer

from pathlib import Path

# check if we have GPU available
print(
    "Setup complete. Using torch"
    + f"{torch.__version__}"
    + f"({torch.cuda.get_device_properties(0).name if torch.cuda.is_available() else 'CPU'}"
    + f" with #{torch.cuda.device_count() if torch.cuda.is_available() else '0'} cuda devices)"
    + f" named {[torch.cuda.get_device_name(x) for x in range(torch.cuda.device_count())] if torch.cuda.is_available() else 'UNKNOWN'}"
)

pd.set_option('display.max_columns', 5000)


def create_data_sets() -> list[pd.DataFrame]:

    train_data = DataPreprocessing.preprocess_user_data(constants.TRAIN_DATA_FOLDER)
    test_data = DataPreprocessing.preprocess_user_data(constants.TEST_DATA_FOLDER)
    validation_data = DataPreprocessing.preprocess_user_data(constants.VALIDATION_DATA_FOLDER)

    return [train_data, test_data, validation_data]


def create_shifted_col(col: list[str],
                       burst_size: int) -> list[str]:
    result = []
    for i in range(len(col)):
        result.append(col[i])
        for j in range(burst_size - 1, 0, -1):
            result.append(col[i] + f" t_{j}")

    return result


def mlp_forecast(train_data_set: pd.DataFrame,
                 test_data_set: pd.DataFrame,
                 validation_data_set: pd.DataFrame,
                 burst_size: int) -> dict[str, np.ndarray]:

    # transform list into array np.asarray()
    train = train_data_set
    # split into input and output columns
    trainX, trainY = train.iloc[:, 1:], train.iloc[:, 0]

    parameter_space = {
        'MLPClassifier__hidden_layer_sizes': [(23, 14, 8),
                                              (64, 23, 14, 8),
                                              (14, 8),
                                              (128, 64, 23, 14, 8),
                                              (70)
                                              ],  #, (20,)
        'MLPClassifier__activation': ['relu'],
        'MLPClassifier__solver': ['adam', 'sgd', ],
        'MLPClassifier__alpha': [0.0001, 0.05],  # 
        'MLPClassifier__learning_rate': ['adaptive'],  # 'constant',
        'MLPClassifier__learning_rate_init': [0.001],
        'MLPClassifier__batch_size': ['auto'],
        'MLPClassifier__early_stopping': [True],
    }

    eye_data = EyeData()
    hand_data = HandData()
    head_data = HeadData()
    instance_seg_data = InstanceSegmentationData()

    eye_features = [item for sub_list in eye_data.get_new_position_labels() for item in sub_list]
    hand_features = [item for sub_list in hand_data.get_new_position_labels() for item in sub_list]
    head_features = [item for sub_list in head_data.get_new_direction_labels() for item in sub_list]

    eye_features_burst_size = len(eye_features) * burst_size
    hand_features_burst_size = len(hand_features) * burst_size
    head_features_burst_size = len(head_features) * burst_size
    user_data_end_index = eye_features_burst_size + hand_features_burst_size + head_features_burst_size

    eye_features_shifted_indices = list(range(eye_features_burst_size))
    hand_features_shifted_indices = list(range(eye_features_burst_size,
                                               eye_features_burst_size + hand_features_burst_size))
    head_features_shifted_indices = list(range(eye_features_burst_size + hand_features_burst_size,
                                               user_data_end_index))

    nn_data_class_labels = instance_seg_data.get_class_labels()
    nn_data_bbox_labels = instance_seg_data.get_bboox_labels()
    bbox_split_factor = 4
    nn_data_class_labels_burst_size = len(nn_data_bbox_labels) * burst_size
    nn_data_bbox_labels_burst_size = len(nn_data_class_labels) * bbox_split_factor * burst_size
    
    # nn_data_class_labels_shifted_indices = list(range(user_data_end_index, user_data_end_index + nn_data_class_labels_burst_size))
    bbox_features_shifted_indices = list(range(user_data_end_index + nn_data_class_labels_burst_size,
                                               user_data_end_index + nn_data_class_labels_burst_size +
                                               nn_data_bbox_labels_burst_size))
    # dataset = TruckDataSet()

    # categorical_transformer = Pipeline(
    #     steps=[
    #         ("encoder", OneHotEncoder(categories=dataset.get_labels(),
    #                                   handle_unknown="ignore")),
    #     ]
    # )

    numeric_transformer = Pipeline(
        steps=[("imputer", SimpleImputer(strategy="median")), ("scaler", StandardScaler())]
    )

    preprocessor = ColumnTransformer(
        transformers=[
            ("numeric_eye", numeric_transformer, eye_features_shifted_indices),
            ("numeric_hand", numeric_transformer, hand_features_shifted_indices),
            ("numeric_head", numeric_transformer, head_features_shifted_indices),
            # ("numeric_bbox", numeric_transformer, bbox_features_shifted_indices),
            # ("class_labels", categorical_transformer, nn_data_class_labels_shifted_indices),
            # ("nn_data", nn_data_transformer, nn_data_shifted_indices),
        ],
        remainder='passthrough'
    )

    # Define a Standard Scaler to normalize inputs
    clf = MLPClassifier(random_state=1)

    pipe = Pipeline(steps=[("preprocessor", preprocessor), ("MLPClassifier", clf), ])

    mlp_clf_grid_search = GridSearchCV(estimator=pipe,
                                       param_grid=parameter_space,
                                       scoring=["neg_log_loss",
                                                "balanced_accuracy",
                                                "f1_micro",
                                                "f1_macro",
                                                ],
                                       refit="neg_log_loss",
                                       n_jobs=-1,
                                       cv=5,
                                       verbose=2,
                                       return_train_score=True,)

    mlp_clf_grid_search.fit(trainX, trainY)

    path = constants.MODELS_DATA_PATH + f"BurstSize_{burst_size}"
    Path(path).mkdir(parents=True, exist_ok=True)
    f = open(path + "/" + "model_results.txt", "w")
    f.write('Best parameters found:\n' + str(mlp_clf_grid_search.best_params_) + "\n")
    f.write('CV results:\n' + str(mlp_clf_grid_search.cv_results_) + "\n")
    # make a one-step prediction, np.asarray()
    test = test_data_set
    testX, testY = test.iloc[:, 1:], test.iloc[:, 0]

    class_labales = TruckDiscoverIntention()

    vis = Visualizer()
    label_binarizer = LabelBinarizer().fit(trainY)

    vis.plot_all_OvR_ROC_curves(mlp_clf_grid_search, testX, testY,
                                class_labales.get_labels(),
                                label_binarizer,
                                burst_size)

    vis.plot_confusion_matrix(mlp_clf_grid_search,
                              testX, testY, burst_size)

    vis.plot_losses(mlp_clf_grid_search, burst_size)

    vis.plot_accuracy(mlp_clf_grid_search, burst_size)

    # transform list into array
    # validation = np.asarray(validation_data_set)
    # split into input and output columns
    # validationX, validationY = validation[:, 1:], validation[:, 0]

    yhat = mlp_clf_grid_search.predict(testX)
    last_f1macro_score = 'Model F1 macro score: {0:0.4f}'. format(f1_score(testY, yhat, average='macro'))
    last_f1micro_score = 'Model F1 micro score: {0:0.4f}'. format(f1_score(testY, yhat, average='micro'))
    last_accuracy_score = 'Model accuracy score: {0:0.4f}'. format(accuracy_score(testY, yhat))

    f.write(last_accuracy_score + "\n" +
            last_f1macro_score + "\n" +
            last_f1micro_score + "\n")

    f.close()

    try:

        print("Last supported opset:", __max_supported_opset__)  # trainX.shape[1]

        initial_types = [('eye data', FloatTensorType([1, eye_features_burst_size])),
                         ('hand data', FloatTensorType([1, hand_features_burst_size])),
                         ('head data', FloatTensorType([1, head_features_burst_size])),
                         ('class labels', FloatTensorType([1, nn_data_class_labels_burst_size])),
                         ]
        # ('bbox results', FloatTensorType([1, nn_data_bbox_labels_burst_size]))

        onx = convert_sklearn(mlp_clf_grid_search,
                              initial_types=initial_types,
                              target_opset={'': 12})

        user_guidance_model_path = path + "/" + "UserGuidanceAIMLP.onnx"

        with open(user_guidance_model_path, "wb") as f:
            f.write(onx.SerializeToString())

    except Exception as e:
        print(e)

    return mlp_clf_grid_search.cv_results_


# def random_forest_forecast(train_data_set: pd.DataFrame, test_data_set: pd.DataFrame) -> None:
#     '''
#     Use a random forest classification for time series
#     '''
#     # transform list into array
#     train = np.asarray(train_data_set)
#     # split into input and output columns
#     trainX, trainY = train[:, 1:], train[:, 0]
    
#     # Number of trees in random forest
#     n_estimators = [int(x) for x in np.linspace(start=200, stop=2000, num=10)]
#     # Number of features to consider at every split
#     max_features = ['auto', 'sqrt']
#     # Maximum number of levels in tree
#     max_depth = [int(x) for x in np.linspace(10, 110, num=11)]
#     max_depth.append(None)
#     # Minimum number of samples required to split a node
#     min_samples_split = [2, 5, 10]
#     # Minimum number of samples required at each leaf node
#     min_samples_leaf = [1, 2, 4]
#     # Method of selecting samples for training each tree
#     bootstrap = [True, False]
#     # Create the random grid
#     random_grid = {'n_estimators': n_estimators,
#                    'max_features': max_features,
#                    'max_depth': max_depth,
#                    'min_samples_split': min_samples_split,
#                    'min_samples_leaf': min_samples_leaf,
#                    'bootstrap': bootstrap}

#     model = make_pipeline(StandardScaler(),
#                           RandomForestClassifier(),
#                           )
#     rf_random = RandomizedSearchCV(estimator=model, param_distributions=random_grid,
#                                    n_iter=1, verbose=2, random_state=42, n_jobs=-1)

#     rf_random.fit(trainX, trainY)

#     best_random_model = rf_random.best_estimator_

#     # make a one-step prediction
#     test = np.asarray(test_data_set)
#     yhat = best_random_model.predict(test[:, 1:])

#     class_labales = TruckDiscoverIntention()

#     vis = Visualizer()

#     # vis.plot_all_OvR_ROC_curves(best_random_model, test[:, 1:], test[:, 0],
#     #                             class_labales.get_labels())

#     print('Model accuracy score with 1000 decision-trees : {0:0.4f}'. format(accuracy_score(test[:, 0], yhat)))

#     initial_types = [('UserInputDataOverSlidingWindow', FloatTensorType([trainX.shape[1]]))]

#     onx = to_onnx(best_random_model,
#                   initial_types=initial_types,
#                   target_opset=7)

#     with open("UserGuidanceRandomForest.onnx", "wb") as f:
#         f.write(onx.SerializeToString())

#     return yhat[0]

# burst size in #samples
BURST_SIZES = [80]  # 100, 300, 400  180, 240  150
train_data_set, test_data_set, validation_data_set = create_data_sets()

results = []

for burst_size in BURST_SIZES:

    train_data_as_time_series = DataPreprocessing.series_to_supervised(train_data_set, burst_size)
    test_data_as_time_series = DataPreprocessing.series_to_supervised(test_data_set, burst_size)
    validation_data_as_time_series = DataPreprocessing.series_to_supervised(validation_data_set, burst_size)

    # random_forest_forecast(train_data_set, test_data_set)
    result = mlp_forecast(train_data_as_time_series,
                          test_data_as_time_series,
                          validation_data_as_time_series,
                          burst_size)

    results.append((burst_size, result))

vis = Visualizer()
vis.compare_different_burst_sizes(results)
