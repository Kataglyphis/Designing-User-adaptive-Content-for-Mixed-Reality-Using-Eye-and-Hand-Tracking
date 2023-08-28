import matplotlib.pyplot as plt
import torch
import numpy as np
import cv2


def main():
    xlist = np.linspace(-6, 6, 1000)

    im = cv2.imread("proto_multiplied_cropped.png",0)
    im[np.greater_equal(im, 255/2)] = 255
    im[np.less(im, 255/2)] = 0
    plt.imshow(im)
    plt.axis('off')
    plt.savefig(f'cropped.png', bbox_inches='tight')
    # plt.show()


if __name__ == '__main__':
    main()