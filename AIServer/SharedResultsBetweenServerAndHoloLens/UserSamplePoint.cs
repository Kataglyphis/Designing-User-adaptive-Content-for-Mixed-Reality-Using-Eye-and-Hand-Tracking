using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

[Serializable]
public class UserSamplePoint
{

    public float[] getUserDataAsArrayPresentation()
    {
        return eyeHitposScreenSpace.Concat(rightIndexTipPosScreenSpace)
                                   .Concat(leftIndexTipPosScreenSpace)
                                   .Concat(headMovement)
                                   .Concat(headVelocity)
                                   .Concat(classLabel)
                                   .Concat(eyeHitPosInOrOut)
                                   .Concat(rightIndexTipPosInOrOut)
                                   .Concat(leftIndexTipPosInOrOut)
                                   .ToArray();
    }

    public void unroll_big_array(float[] all_data)
    {

        eyeHitposScreenSpace[0] = all_data[0];
        eyeHitposScreenSpace[1] = all_data[1];

        rightIndexTipPosScreenSpace[0] = all_data[2];
        rightIndexTipPosScreenSpace[1] = all_data[3];

        leftIndexTipPosScreenSpace[0] = all_data[4];
        leftIndexTipPosScreenSpace[1] = all_data[5];

        headMovement[0] = all_data[6];
        headMovement[1] = all_data[7];
        headMovement[2] = all_data[8];

        headVelocity[0] = all_data[9];
        headVelocity[1] = all_data[10];
        headVelocity[2] = all_data[11];

        classLabel[0] = all_data[12];
        classLabel[1] = all_data[13];
        classLabel[2] = all_data[14];
        classLabel[3] = all_data[15];

        eyeHitPosInOrOut[0] = all_data[16];
        eyeHitPosInOrOut[1] = all_data[17];
        eyeHitPosInOrOut[2] = all_data[18];
        eyeHitPosInOrOut[3] = all_data[19];

        rightIndexTipPosInOrOut[0] = all_data[20];
        rightIndexTipPosInOrOut[1] = all_data[21];
        rightIndexTipPosInOrOut[2] = all_data[22];
        rightIndexTipPosInOrOut[3] = all_data[23];

        leftIndexTipPosInOrOut[0] = all_data[24];
        leftIndexTipPosInOrOut[1] = all_data[25];
        leftIndexTipPosInOrOut[2] = all_data[26];
        leftIndexTipPosInOrOut[3] = all_data[27];

    }

    public List<float> eyeHitposScreenSpace = new List<float> { 0.0f, 0.0f };

    public List<float> rightIndexTipPosScreenSpace = new List<float> { 0.0f, 0.0f };

    public List<float> leftIndexTipPosScreenSpace = new List<float> { 0.0f, 0.0f };

    public List<float> headMovement = new List<float> { 0.0f, 0.0f, 0.0f };

    public List<float> headVelocity = new List<float> { 0.0f, 0.0f, 0.0f };

    public List<float> classLabel = new List<float> { 0.0f, 0.0f, 0.0f, 0.0f };

    public List<float> eyeHitPosInOrOut = new List<float> { 0.0f, 0.0f, 0.0f, 0.0f };

    public List<float> rightIndexTipPosInOrOut = new List<float> { 0.0f, 0.0f, 0.0f, 0.0f };

    public List<float> leftIndexTipPosInOrOut = new List<float> { 0.0f, 0.0f, 0.0f, 0.0f };

}

