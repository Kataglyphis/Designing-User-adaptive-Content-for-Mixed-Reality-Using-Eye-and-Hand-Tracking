import matplotlib.pyplot as plt
import torch
import numpy as np


def SiLU(x):
    return x * torch.sigmoid(x)


def ReLU(x):
    return torch.max(torch.zeros_like(x), x)

def tanh(x):
    return torch.tanh(x)

def main():
    xlist = np.linspace(-3, 3, 1000)

    plt.figure()
    plt.axhline(y=0, color="black", linestyle="--")
    plt.axvline(color="black", linestyle="--")
    plt.plot(xlist, SiLU(torch.tensor(xlist)), label='SiLU', color=(105/255, 240/255, 174/255))
    plt.plot(xlist, ReLU(torch.tensor(xlist)), label='ReLU', color=(0.58, 0, 0.82))
    plt.plot(xlist, tanh(torch.tensor(xlist)), label='Tanh', color=(1.0, 0.49, 0.0))
    plt.legend()
    plt.show()


if __name__ == '__main__':
    main()
