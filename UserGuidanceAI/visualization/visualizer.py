import numpy as np
import pandas as pd
from pandas import Grouper
from data.nn_data.datasets.truck_dataset import TruckDataSet
from data.nn_data.instance_segmentation_data import InstanceSegmentationData
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.patches as patches

import torch
import torch.nn.functional as F
from pathlib import Path

from data.user_data.eye_data import EyeData
from data.user_data.hand_data import HandData
from data.user_data.head_data import HeadData
from math_helper.math_helper import MathHelper
import constants

from sklearn import metrics
from sklearn.preprocessing import LabelBinarizer
from sklearn.metrics import roc_curve, auc, RocCurveDisplay, roc_auc_score, confusion_matrix, ConfusionMatrixDisplay

from itertools import cycle


class Visualizer():

    def __init__(self) -> None:
        pass

    def visualize_head_data(self,
                            df_tracking_data_preprocessed: pd.DataFrame,
                            head_labels: list[list[str]]) -> None:

        first_intention = df_tracking_data_preprocessed.loc[df_tracking_data_preprocessed["SessionType"] == 1]
        second_intention = df_tracking_data_preprocessed.loc[df_tracking_data_preprocessed["SessionType"] == 2]
        third_intention = df_tracking_data_preprocessed.loc[df_tracking_data_preprocessed["SessionType"] == 3]

        fig = plt.figure()
        ax = fig.add_subplot(projection='3d')
        index = 0
        image_index = 0

        x_label = "x - coord"
        x_range = [-1, 1]
        y_label = "y - coord"
        y_range = [-1, 1]
        z_label = "z - coord"
        z_range = [-1, 1]

        labels = ["FirstIntention", "SecondIntention", "ThirdIntention"]
        marker = ['o', 'o', 'o']
        colors = ["aqua", "fuchsia", "lawngreen"]
        marker_size = 1

        ax.scatter(first_intention[head_labels[0][0]], first_intention[head_labels[0][2]], first_intention[head_labels[0][1]],
                   marker=marker[0],
                   s=marker_size,
                   color=colors[0],
                   label=labels[0])
        
        ax.scatter(second_intention[head_labels[0][0]], second_intention[head_labels[0][2]], second_intention[head_labels[0][1]],
                   marker=marker[1],
                   s=marker_size,
                   color=colors[1],
                   label=labels[1])
        
        ax.scatter(third_intention[head_labels[0][0]], third_intention[head_labels[0][2]], third_intention[head_labels[0][1]],
                   marker=marker[2],
                   s=marker_size,
                   color=colors[2],
                   label=labels[2])

        ax.set_xlabel(x_label)
        ax.set_xlim(x_range)
        ax.set_ylabel(z_label)
        ax.set_ylim(z_range)
        ax.set_zlabel(y_label)
        ax.set_zlim(y_range)
        plt.title(f"Head data from one user")

        path = constants.VISUALIZER_IMAGE_PATH + "AllUsers/Head"
        Path(path).mkdir(parents=True, exist_ok=True)
        
        plt.legend(loc='upper center', bbox_to_anchor=(0.5, -0.05),
                    fancybox=True, shadow=True, ncol=4)
        plt.show()
        plt.savefig(path + "/" + f"HeadData.png")
        plt.close()

    def visualize_all_2d_positions_screen_space(self,
                                                df_tracking_data_preprocessed: pd.DataFrame,
                                                position_labels: list[list[str]],
                                                user_name: str,
                                                intention: str,
                                                marker: list[str] = ['o', 'x', 'x'],
                                                visualize_bursts: bool = True,
                                                step_size: int = 5,
                                                offset: int = 0,
                                                colors: list[str] = ['b', 'g', 'r'],
                                                visualize_3d: bool = True,
                                                burst_size: int = constants.SLIDING_WINDOW_SIZE) -> None:

        labels = ["Eye", "Right Hand", "Left Hand"]

        index = 0
        image_index = 0
        while ((index + burst_size) < df_tracking_data_preprocessed.shape[0]):

            if visualize_3d:

                fig = plt.figure()
                ax = fig.add_subplot(projection='3d')

            for position_label_index, position_label in enumerate(position_labels):

                positions = df_tracking_data_preprocessed[position_label].to_numpy()

                correct_x_positions = np.logical_and(positions[:, 0] >= -1, positions[:, 0] <= 1)
                correct_y_positions = np.logical_and(positions[:, 1] >= -1, positions[:, 1] <= 1)
                correct_positions = np.logical_and(correct_x_positions, correct_y_positions)


                positions = positions[correct_positions]
                # we ignore the very first and very last samples
                positions = positions[offset:-offset]
                if (positions.shape[0] == 0):
                    continue
                position_x = positions[:, 0]
                position_y = positions[:, 1]

                from_index = index
                to_index = from_index + burst_size
                burst_position_x = position_x[from_index:to_index]
                burst_position_y = position_y[from_index:to_index]

                # here go from screen space to NDC
                burst_position_x = ((burst_position_x + 1) / 2) * constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_X
                burst_position_y = ((burst_position_y + 1) / 2) * constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y

                x_label = "x - coord"
                x_range = [0, constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_X]
                y_label = "y - coord"
                y_range = [0, constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y]

                if visualize_3d:

                    time_axis = np.arange(len(burst_position_x))

                    ax.scatter(burst_position_x, time_axis, burst_position_y,
                               marker=marker[position_label_index],
                               color=colors[position_label_index],
                               label=labels[position_label_index])

                    ax.set_xlabel(x_label)
                    ax.set_xlim(x_range)
                    ax.set_ylabel("time")
                    # ax.set_ylim(y_range)
                    ax.set_zlabel(y_label)
                    ax.set_zlim(y_range)
                    plt.title(f"User data with {intention} over #{burst_size} samples burst")

                else:
                    
                    plt.title(f"Accumulated user data with {intention} over #{burst_size} samples burst")
                    plt.xlabel(x_label)
                    plt.ylabel(y_label)
                    plt.xlim(x_range)
                    plt.ylim(y_range)
                    plt.scatter(burst_position_x, burst_position_y,
                                marker=marker[position_label_index],
                                color=colors[position_label_index],
                                label=labels[position_label_index])


            path = constants.VISUALIZER_IMAGE_PATH + user_name + "/" + intention + "/" + "Bursts"
            Path(path).mkdir(parents=True, exist_ok=True)
            
            plt.legend(loc='upper center', bbox_to_anchor=(0.5, -0.05),
                       fancybox=True, shadow=True, ncol=4)
            plt.savefig(path + "/" + f"Accumulated user data from {user_name} with intention {intention} burst {image_index}.png")
            plt.close()
            index += step_size
            image_index += 1

    def compare_different_burst_sizes(self,
                                      different_burst_sizes) -> None:

        path = constants.MODELS_DATA_PATH
        Path(path).mkdir(parents=True, exist_ok=True)

        x = []
        y = []
        for index in range(len(different_burst_sizes)):
            x.append(different_burst_sizes[index][0])
            y.append(different_burst_sizes[index][1]["mean_test_balanced_accuracy"])

        plt.title("Compare different burst sizes")
        plt.plot(x, y, label='compare burst sizes')
        plt.legend(loc='best')
        # plt.show()
        plt.savefig(path + "/" + "BurstSizesCompare.png")
        plt.close()



    def plot_losses(self, clf, burst_size: int) -> None:

        path = constants.MODELS_DATA_PATH + f"BurstSize_{burst_size}"
        Path(path).mkdir(parents=True, exist_ok=True)
        loss_curve = clf.best_estimator_[1].loss_curve_

        plt.title("Training losses")
        plt.plot(loss_curve, label='losses')
        plt.legend(loc='best')
        # plt.show()
        plt.savefig(path + "/" + "Losses.png")
        plt.close()

    def plot_accuracy(self, clf, burst_size: int) -> None:

        path = constants.MODELS_DATA_PATH + f"BurstSize_{burst_size}"
        Path(path).mkdir(parents=True, exist_ok=True)

        colors = ["aqua", "darkorange", "cornflowerblue", "lawngreen"]

        validation_scores = clf.best_estimator_[1].validation_scores_

        plt.plot(validation_scores, label='validation scores', color=colors[0])

        plt.title("Accuracy")
        plt.legend(loc='best')
        # plt.show()
        plt.savefig(path + "/" + "Accuracy.png")
        plt.close()


    def plot_confusion_matrix(self, clf, X_test, y_test, burst_size: int) -> None:

        predictions = clf.predict(X_test)
        cm = confusion_matrix(y_test, predictions, labels=clf.classes_)
        disp = ConfusionMatrixDisplay(confusion_matrix=cm,
                                      display_labels=clf.classes_)
        disp.plot()
        path = constants.MODELS_DATA_PATH + f"BurstSize_{burst_size}"
        Path(path).mkdir(parents=True, exist_ok=True)
        # plt.show()
        plt.savefig(path + "/" + "ConfusionMatrix.png")
        plt.close()


    def plot_all_OvR_ROC_curves(self, model, testX, testY,
                                class_labels: list[str],
                                label_binarizer,
                                burst_size: int) -> None:

        y_score = model.predict_proba(testX)
        n_classes = 4

        fig, ax = plt.subplots(figsize=(6, 6))

        y_onehot_test = label_binarizer.transform(testY)

        fpr, tpr, roc_auc = dict(), dict(), dict()
        # Compute micro-average ROC curve and ROC area
        aux1 = testY.ravel()
        aux2 = y_score.ravel()
        fpr["micro"], tpr["micro"], _ = roc_curve(y_onehot_test.ravel(), y_score.ravel())
        roc_auc["micro"] = auc(fpr["micro"], tpr["micro"])

        plt.plot(
            fpr["micro"],
            tpr["micro"],
            label=f"micro-average ROC curve (AUC = {roc_auc['micro']:.2f})",
            color="deeppink",
            linestyle=":",
            linewidth=4,
        )

        for i in range(n_classes):

            fpr[i], tpr[i], _ = roc_curve(y_onehot_test[:, i], y_score[:, i])
            roc_auc[i] = auc(fpr[i], tpr[i])

        fpr_grid = np.linspace(0.0, 1.0, 1000)

        # Interpolate all ROC curves at these points
        mean_tpr = np.zeros_like(fpr_grid)

        for i in range(n_classes):
            mean_tpr += np.interp(fpr_grid, fpr[i], tpr[i])  # linear interpolation

        # Average it and compute AUC
        mean_tpr /= n_classes

        fpr["macro"] = fpr_grid
        tpr["macro"] = mean_tpr
        roc_auc["macro"] = auc(fpr["macro"], tpr["macro"])

        plt.plot(
            fpr["macro"],
            tpr["macro"],
            label=f"macro-average ROC curve (AUC = {roc_auc['macro']:.2f})",
            color="navy",
            linestyle=":",
            linewidth=4,
        )

        colors = cycle(["aqua", "darkorange", "cornflowerblue", "lawngreen"])
        for class_id, color in zip(range(n_classes), colors):
            RocCurveDisplay.from_predictions(
                    testY,
                    y_score[:, class_id],
                    name=f"{class_labels[class_id]} vs the rest",
                    color=color,
                    pos_label=class_id,
                    ax=ax,
            )

        plt.plot([0, 1], [0, 1], "k--", label="chance level (AUC = 0.5)")
        plt.axis("square")
        plt.xlabel("False Positive Rate")
        plt.ylabel("True Positive Rate")
        plt.title("One-vs-Rest ROC curves:\n")
        plt.legend()
        path = constants.MODELS_DATA_PATH + f"BurstSize_{burst_size}"
        Path(path).mkdir(parents=True, exist_ok=True)
        # plt.show()
        plt.savefig(path + "/" + "One-vs-Rest ROC curves.png")
        plt.close()


    def visualize_results(self,
                          df_tracking_data_old: pd.DataFrame,
                          df_tracking_data_preprocessed: pd.DataFrame,
                          eye_data: EyeData,
                          in_or_out_labels_eye_hit_pos: list[str],
                          hand_data: HandData,
                          in_or_out_labels_hand_pos: list[str],
                          head_data: HeadData,
                          instance_segmentation_data: InstanceSegmentationData,
                          masks: np.array,
                          user_name: str,
                          intention: str,
                          columns=1,
                          rows=1,
                          max_number_of_batches=50) -> None:
        """
        For data understanding and debugging purposes
        """
        mask_columns_joined = instance_segmentation_data.get_joined_mask_columns()
        class_labels = instance_segmentation_data.get_class_labels()
        class_probs = instance_segmentation_data.get_prob_labels()

        eye_hit_Pos_screen_space = eye_data.get_new_position_labels()[0]
        right_index_tip_Pos_screen_space = hand_data.get_new_position_labels()[0]
        left_index_tip_Pos_screen_space = hand_data.get_new_position_labels()[1]

        matplotlib.use('Agg')

        # batch some masks together in some plot
        batch_size = (columns * rows)
        # iterate over time
        saved_fig = 0
        for u in range(masks.shape[1] // batch_size):

            plt.axis('off')
            fig, axs = plt.subplots(rows, columns, figsize=(15, 15), squeeze=False)

            dataset = TruckDataSet()
            labels = dataset.get_labels()

            # plot multiple time steps at once
            for i in range(0, rows):
                for j in range(0, columns):

                    axs[i, j].invert_yaxis()
                    # ax1 = fig.add_subplot(rows, columns, i)
                    global_index = u * batch_size + (i * columns + j)

                    img_count = 0

                    # iterate over all mask of a time step
                    for m in range(len(mask_columns_joined)):

                        # all masks from the #m instance segmentation mask
                        masks_of_segmentation_result_m = masks[m]
                        class_labels_of_segmentation_result_m = df_tracking_data_old[class_labels[m]]
                        class_probs_of_segmentation_result_m = df_tracking_data_old[class_probs[m]]

                        if (labels[class_labels_of_segmentation_result_m[global_index]] != "platform"):
                            continue

                        img_count += 1

                        eye_hit_pos_in_or_out = df_tracking_data_preprocessed[in_or_out_labels_eye_hit_pos[0][m]].iloc[global_index]
                        right_index_tip_pos_in_or_out = df_tracking_data_preprocessed[in_or_out_labels_hand_pos[0][m]].iloc[global_index]
                        left_index_tip_pos_in_or_out = df_tracking_data_preprocessed[in_or_out_labels_hand_pos[1][m]].iloc[global_index]

                        window_shape = (constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y, constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_X)
                        torch_resized = F.interpolate(input=torch.from_numpy(masks_of_segmentation_result_m[global_index])[None, None], size=window_shape, mode='bilinear', align_corners=False)[0]
                        img = axs[i, j].imshow(torch_resized[0].numpy(), origin='upper')

                    if (img_count == 0):
                        break

                    plt.title(f"Segmentation masks \n with projected user data in screen space \nt = {u}", fontsize=20)
                    plt.colorbar(img, ax=axs[i, j], orientation='horizontal')

                    eye_hit_pos_ndc = MathHelper.screen_space_to_ndc(np.array([(float)(df_tracking_data_preprocessed[eye_hit_Pos_screen_space[0]][global_index]),
                                                                            (float)(df_tracking_data_preprocessed[eye_hit_Pos_screen_space[1]][global_index])],
                                                                            dtype=float),
                                                                        np.array([constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_X,
                                                                            constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y],
                                                                            dtype=int))

                    right_index_tip_pos_ndc = MathHelper.screen_space_to_ndc(np.array([(float)(df_tracking_data_preprocessed[right_index_tip_Pos_screen_space[0]][global_index]),
                                                                            (float)(df_tracking_data_preprocessed[right_index_tip_Pos_screen_space[1]][global_index])],
                                                                            dtype=float),
                                                                    np.array([constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_X,
                                                                            constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y],
                                                                            dtype=int))

                    left_index_tip_pos_ndc = MathHelper.screen_space_to_ndc(np.array([(float)(df_tracking_data_preprocessed[left_index_tip_Pos_screen_space[0]][global_index]),
                                                                            (float)(df_tracking_data_preprocessed[left_index_tip_Pos_screen_space[1]][global_index])],
                                                                            dtype=float),
                                                                    np.array([constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_X,
                                                                            constants.MAX_RENDER_TARGET_SIZE_HOLOLENS2_Y],
                                                                            dtype=int))

                    marker_size = 500
                    axs[i, j].scatter(eye_hit_pos_ndc[0],
                                        eye_hit_pos_ndc[1],
                                        color='ghostwhite',
                                        s=marker_size,
                                        label=eye_hit_Pos_screen_space[0].split('(')[0])

                    axs[i, j].scatter(right_index_tip_pos_ndc[0],
                                        right_index_tip_pos_ndc[1],
                                        color='orangered',
                                        marker='x',
                                        s=marker_size,
                                        label=right_index_tip_Pos_screen_space[0].split('(')[0])

                    axs[i, j].scatter(left_index_tip_pos_ndc[0],
                                        left_index_tip_pos_ndc[1],
                                        color='g',
                                        marker='x',
                                        s=marker_size,
                                        label=left_index_tip_Pos_screen_space[0].split('(')[0])

                    axs[i, j].set_xlabel('x-coord')
                    axs[i, j].set_ylabel('y-coord')
                    axs[i, j].legend(loc='lower left', fontsize='xx-large')

            fig.tight_layout()

            if (img_count == 0):
                continue

            path = constants.VISUALIZER_IMAGE_PATH + user_name + "/" + intention

            Path(path).mkdir(parents=True, exist_ok=True)

            plt.savefig(path + "/" + f"instance_segmentation_masks_t_{saved_fig}.png")
            saved_fig += 1

            plt.clf()
            plt.cla()
            plt.close(fig)
