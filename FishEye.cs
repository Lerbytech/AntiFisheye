using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using Emgu;

namespace AntiFisheye
{
  public class FishEye
  {
    // 700 / 459 / 350 / 250 / 145 (Разнес на отдельные функции) / 115 (убрал лишние переменные)
    //решить как лучше хранить адрес перадресовки
    // перешел на data - 15мс
    //отныне усреднение не по 10, а по 500 шагов
    // сменил порядок итерации высота/ширина - 7мс 


    private ArrayList mFisheyeCorrect; 
    private int mFELimit = 1500;
    private double mScaleFESize = 0.9;
    private int[,] XD_Table;
    private int[,] YD_Table;

    public FishEye()
    {
      //A lookup table so we don't have to calculate Rdistorted over and over
      //The values will be multiplied by focal length in pixels to 
      //get the Rdistorted
      mFisheyeCorrect = new ArrayList(mFELimit);
      //it corresponds to Rundist/focalLengthInPixels * 1000 (to get integers)
      double result;
      for (int i = 0; i < mFELimit; i++)
      {
        result = Math.Sqrt(1 - 1 / Math.Sqrt(1.0 + (double)i * i / 1000000.0)) * 1.4142136;
        mFisheyeCorrect.Add(result);
      }
    }

    // разбить на 4 квадранта
    public void CalculateParameters(Image<Gray, Byte> inputImg, double aFocalLinPixels)
    {
      int width = inputImg.Width;
      int height = inputImg.Height;
      double xc = width / 2.0;
      double yc = height / 2.0;
      Boolean xpos, ypos;
      double xdif;
      double ydif;
      double Rusquare;
      double theta;
      int index;
      double Rd;
      double xdelta;
      double ydelta;
      int xd;
      int yd;
      int i = 0;
      int j = 0;

      XD_Table = new int[height, width];
      YD_Table = new int[height, width];
      //Move through the pixels in the corrected image; 
      //set to corresponding pixels in distorted image
      for (j = 0; j < height; j++)
      {
        for (i = 0; i < width; i++)
        {
          //which quadrant are we in?
          xpos = i > xc;
          ypos = j > yc;
          //Find the distance from the center
          xdif = i - xc;
          ydif = j - yc;
          //The distance squared
          Rusquare = xdif * xdif + ydif * ydif;
          //the angle from the center
          theta = Math.Atan2(ydif, xdif);
          //find index for lookup table
          index = (int)(Math.Sqrt(Rusquare) / aFocalLinPixels * 1000);
          if (index >= mFELimit) index = mFELimit - 1;
          //calculated Rdistorted
          Rd = aFocalLinPixels * (double)mFisheyeCorrect[index]
                                / mScaleFESize;
          //calculate x and y distances
          xdelta = Math.Abs(Rd * Math.Cos(theta));
          ydelta = Math.Abs(Rd * Math.Sin(theta));
          //convert to pixel coordinates
          xd = (int)(xc + (xpos ? xdelta : -xdelta));
          yd = (int)(yc + (ypos ? ydelta : -ydelta));
          xd = Math.Max(0, Math.Min(xd, inputImg.Width - 1));
          yd = Math.Max(0, Math.Min(yd, inputImg.Height - 1));
          XD_Table[j, i] = xd;
          YD_Table[j, i] = yd;
          //set the corrected pixel value from the distorted image
          //correctedImage.SetPixel(i, j, aImage.GetPixel(xd, yd));
          //correctedImage[j, i] = new Gray(sourceImage[yd, xd].Intensity);
          //correctedImage[j, i] = new Gray(sourceImage[j, i].Intensity);
        }
      }
    }


    public Image<Gray, Byte> RemoveFisheye(Image<Gray, Byte> aImage, double aFocalLinPixels)
    {
      Image<Gray, Byte> correctedImage = new Image<Gray, Byte>(aImage.Width, aImage.Height, new Gray(0));
      Image<Gray, Byte> sourceImage = aImage;

      byte[, ,] correctedImage_Data = correctedImage.Data;
      byte[, ,] sourceImage_Data = sourceImage.Data;

      //The center points of the image
      //double xc = aImage.Width / 2.0;
      //double yc = aImage.Height / 2.0;
      //Boolean xpos, ypos;
      //double xdif;
      //double ydif;
      //double Rusquare;
      //double theta;
      //int index;
      //double Rd;
      //double xdelta;
      //double ydelta;
      //int xd;
      //int yd;
      int width = correctedImage.Width;
      int height = correctedImage.Height;
      int i = 0;
      int j = 0;
      //Move through the pixels in the corrected image; 
      //set to corresponding pixels in distorted image
      
        for (j = 0; j < height; j++)
        {
          for (i = 0; i < width; i++)
          {
            //which quadrant are we in?
            //xpos = i > xc;
            //ypos = j > yc;
            ////Find the distance from the center
            //xdif = i - xc;
            //ydif = j - yc;
            ////The distance squared
            //Rusquare = xdif * xdif + ydif * ydif;
            ////the angle from the center
            //theta = Math.Atan2(ydif, xdif);
            ////find index for lookup table
            //index = (int)(Math.Sqrt(Rusquare) / aFocalLinPixels * 1000);
            //if (index >= mFELimit) index = mFELimit - 1;
            ////calculated Rdistorted
            //Rd = aFocalLinPixels * (double)mFisheyeCorrect[index]
            //                      / mScaleFESize;
            ////calculate x and y distances
            //xdelta = Math.Abs(Rd * Math.Cos(theta));
            //ydelta = Math.Abs(Rd * Math.Sin(theta));
            ////convert to pixel coordinates
            //xd = (int)(xc + (xpos ? xdelta : -xdelta));
            //yd = (int)(yc + (ypos ? ydelta : -ydelta));
            //xd = Math.Max(0, Math.Min(xd, aImage.Width - 1));
            //yd = Math.Max(0, Math.Min(yd, aImage.Height - 1));
            ////set the corrected pixel value from the distorted image
            //correctedImage.SetPixel(i, j, aImage.GetPixel(xd, yd));
            //correctedImage[j, i] = new Gray(sourceImage[yd, xd].Intensity);

            //correctedImage[j, i] = new Gray(sourceImage[YD_Table[j, i], XD_Table[j, i]].Intensity);
            correctedImage_Data[j,i,0] = sourceImage_Data[YD_Table[j, i], XD_Table[j, i], 0];
          }
        }
      
      return correctedImage;
    }

    public Image<Gray, Byte> Test(Image<Gray, Byte> inputImg, double aFocalLinPixels)
    {
      /*
      Bitmap b = aImage.Bitmap;
      BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
      System.IntPtr Scan0 = bmData.Scan0;
      */

      Image<Gray, Byte> resultImg = inputImg.CopyBlank();

      MIplImage MIpImg1 = (MIplImage)System.Runtime.InteropServices.Marshal.PtrToStructure(inputImg.Ptr, typeof(MIplImage));
      MIplImage MIpImg2 = (MIplImage)System.Runtime.InteropServices.Marshal.PtrToStructure(resultImg.Ptr, typeof(MIplImage));


      int imageHeight = MIpImg1.Height;
      int imageWidth = MIpImg1.Width;
      unsafe
      {
        byte* npixel1 = (byte*)MIpImg1.ImageData;
        byte* npixel2 = (byte*)MIpImg2.ImageData;
        for (int y = 0; y < imageHeight; y++)
        {
          for (int x = 0; x < imageWidth; x++)
          {
            
            npixel2[0] = npixel1[0];
            //npixel1[0] = XD_Table[width] ;  //blue
            //npixel1[0] = 255;  //green
            //npixel[2] = 255;  //red
            //npixel1++;
            npixel2++;
            npixel1 = (byte*)(imageWidth * YD_Table[y, x] + XD_Table[y, x]);
          }
        }
      }

      return resultImg;
    }
  }

}
