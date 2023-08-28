import matplotlib.pyplot as plt
import torch
import numpy as np
import cv2
from scipy import stats


def main():

    #scales = ['1', '2', '3', '4', '5', '6', '7']
    scales = [1, 2, 3, 4, 5, 6, 7]
    counts = [0, 0, 3, 3, 5, 3, 0]
    values = [3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 5, 6, 6, 6]
    total_amount = sum(counts)

    fig, ax = plt.subplots()
    bar_container = ax.bar(scales, counts)
    ax.set(ylabel='counts', title='Does the app detect your intention?', ylim=(0, 6))
    ax.bar_label(
        bar_container, fmt=lambda x: '{:.0f}%'.format((x / total_amount)*100)
    )
    # bar_labels = ['0%','0%','0%','0%','0%','0%','0%']
    # bar_colors = ['tab:blue', 'tab:blue', 'tab:blue', 'tab:blue']

    # for scales_index, scales in enumerate(scales):
    #     p = ax.bar(scales, counts[scales_index])
    #     ax.bar_label(p, labels=bar_labels[scales_index], label_type='center')

    # # ax.bar(scales, counts,  color=bar_colors)

    # ax.set_ylabel('counts')
    # ax.set_title('Does the app detect your intention?')

    # ax.legend(title='Fruit color')
    mean, std = stats.norm.fit(values)
    x = np.linspace(0, 8, 100)
    y = stats.norm.pdf(x, mean, std)
    # plt.plot(x, y*(len(values)-1), color='black', linewidth=2, label='Gaussian fit')
    plt.show()
    #plt.savefig(f'survey_quality_intentions_detection.png')


if __name__ == '__main__':
    main()
