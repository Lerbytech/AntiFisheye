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

  /// <summary>
  /// Классы для поиска барной стойки. Все классы возвращают ч/б изображение - МАСКУ!!!
  /// Возвращаемая маска, очевидно, имеет такой же размер, как и исходное изображение
  /// Маску следует наложить на исходное изображение и будет щастье.
  /// </summary>
  /// 

  public enum Orientation { Top, Bottom, Left, Right};
  public static class BarTableDetector
  {
    // не работает, дебаг
    public static Image<Gray, Byte> LineBasedDetection(Image<Gray, Byte> inputImg)
    {
      Image<Gray, Byte> smoothedImg = inputImg.SmoothMedian(3);
      //CvInvoke.CLAHE(smoothedImg, 40, new System.Drawing.Size(16, 16), smoothedImg);
      CvInvoke.BilateralFilter(inputImg.SmoothMedian(3), smoothedImg, 50, 140, -1);
      smoothedImg = smoothedImg.ThresholdToZero( new Gray(30));

#region Kernels and stuff
      float[,] TopKernelMatrix = new float[,] { { 0, -1, 0}, 
                                                 { 0,  0, 0}, 
                                                 { 0,  1, 0,}};
      float[,] BottomKernelMatrix = new float[,] { { 0, 1, 0}, 
                                                 { 0,  0, 0}, 
                                                 { 0,  -1, 0,}};
      float[,] LeftKernelMatrix = new float[,] { { 0,  0, 0}, 
                                                 { 1,  0, -1}, 
                                                 { 0,  0, 0,}};
      float[,] RightKernelMatrix = new float[,] { { 0, 0, 0}, 
                                                 { -1,  0, 1}, 
                                                 { 0,  0, 0,}};
      ConvolutionKernelF TopKernel = new ConvolutionKernelF(TopKernelMatrix);
      ConvolutionKernelF BottomKernel = new ConvolutionKernelF(BottomKernelMatrix);
      ConvolutionKernelF LeftKernel = new ConvolutionKernelF(LeftKernelMatrix);
      ConvolutionKernelF RightKernel = new ConvolutionKernelF(RightKernelMatrix);
#endregion

#region Craft top / bottom / left and right images with borders
      Image<Gray, Byte> TopImg = new Image<Gray, byte>(inputImg.Size);
      Image<Gray, Byte> BottomImg = new Image<Gray, byte>(inputImg.Size);
      Image<Gray, Byte> LeftImg = new Image<Gray, byte>(inputImg.Size);
      Image<Gray, Byte> RightImg = new Image<Gray, byte>(inputImg.Size);

      
      CvInvoke.Filter2D(smoothedImg, TopImg, TopKernel, new Point(1, 1));
      CvInvoke.Filter2D(smoothedImg, BottomImg, BottomKernel, new Point(1, 1));
      CvInvoke.Filter2D(smoothedImg, LeftImg, LeftKernel, new Point(1, 1));
      CvInvoke.Filter2D(smoothedImg, RightImg, RightKernel, new Point(1, 1));

      TopImg = TopImg.ThresholdBinary(new Gray(15), new Gray(255));
      BottomImg = BottomImg.ThresholdBinary(new Gray(15), new Gray(255));
      LeftImg = LeftImg.ThresholdBinary(new Gray(15), new Gray(255));
      RightImg = RightImg.ThresholdBinary(new Gray(15), new Gray(255));
#endregion

      # region Находим контуры для всех 4х изображений

      VectorOfPoint TopContour = FindMaxContour(TopImg, Orientation.Top);
      VectorOfPoint BottomContour = FindMaxContour(BottomImg, Orientation.Bottom);
      VectorOfPoint LeftContour = FindMaxContour(LeftImg, Orientation.Left);
      VectorOfPoint RightContour = FindMaxContour(RightImg, Orientation.Right);

      VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
      //contours.Push(TopContour);
      //contours.Push(BottomContour);
      //contours.Push(LeftContour);
      //contours.Push(RightContour);
      
      // Отрисовка в целях отладки
      Image<Bgr, Byte> bgrimg = new Image<Bgr,byte>(@"C:\Users\Админ\Downloads\image63341262,2002.png");
      CvInvoke.DrawContours(bgrimg, contours, -1, new MCvScalar(255, 0, 255));
      #endregion

      //находим по три точки каждого контура - концы и внешний центр
        Point[] BorderPoints = FindExtremeContoursPoints(contours);

      //ppp = ppp + BottomImg + LeftImg + RightImg;
      //CvInvoke.Line(bgrimg, BorderPoints[0], BorderPoints[8], new MCvScalar(0, 0, 255));
      //CvInvoke.Line(bgrimg, BorderPoints[2], BorderPoints[6], new MCvScalar(0, 0, 255));

      //теперь зная все контуры и их точки, можем получить маску.
      // здесь использован самы простой метод - берутся только верхняя и нижняя линии, как самые простые в распознавании
      // имхо, в продакшене следует брать боковые, но нужно обсудить пару нюансов

      Image<Gray, Byte> Mask = inputImg.CopyBlank();
      VectorOfVectorOfPoint MaskVoVoP = new VectorOfVectorOfPoint();
      MaskVoVoP.Push(TopContour); MaskVoVoP.Push(BottomContour);
      CvInvoke.DrawContours(Mask, MaskVoVoP, -1, new MCvScalar(255), -1);
      CvInvoke.Line(Mask, BorderPoints[0], BorderPoints[8], new MCvScalar(255));
      CvInvoke.Line(Mask, BorderPoints[2], BorderPoints[6], new MCvScalar(255));

      //заливку как то геморно настраивать, поэтому следующие четыре строчки - костыль. мне стыдно :D
      VectorOfVectorOfPoint finalVector = new VectorOfVectorOfPoint();
      CvInvoke.FindContours(Mask, finalVector, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
      Mask = Mask.CopyBlank();
      CvInvoke.DrawContours(Mask, finalVector, -1, new MCvScalar(255), -1);
      
      //VectorOfPoint externalBorder = finalVector[0];
      //int cX = 0;
      //int cY = 0;
      //for (int i = 0; i < finalVector[0].Size; i++)
      //{ cX += finalVector[0][i].X; cY += finalVector[0][i].Y; }
      //cX /= finalVector[0].Size; cY /= finalVector[0].Size;
      //Point Center = new Point(cX, cY);

      //double k;      double b;
      //k = (TopLeft.Y - BottomLeft.Y) / (TopLeft.X - BottomLeft.X);
      //b = TopLeft.Y - k * TopLeft.X;
      //Point LeftMedium = new Point( (int)Math.Round((cY - b) / k), cY);

      //k = (TopRight.Y - BottomRight.Y) / (TopRight.X - BottomRight.X);
      //b = TopRight.Y - k * TopRight.X;
      //Point RightMedium = new Point( (int)Math.Round((cY - b) / k), cY);

      //Image<Gray, Byte> TableImg = SRC_Img.Copy(Mask);
      //bgrimg.Draw(new CircleF(new PointF(cX, cY), 2), new Bgr(255, 0, 255), 1);
      //bgrimg.Draw(new CircleF(LeftMedium, 2), new Bgr(255, 0, 0), 1);
      //bgrimg.Draw(new CircleF(RightMedium, 2), new Bgr(255, 0, 0), 1);
      
      //Image<Gray, Byte> TL_Img = CropImg(SRC_Img, new Point[] { TopLeft, TopMedium, Center, LeftMedium });
      //Image<Gray, Byte> TR_Img = CropImg(SRC_Img, new Point[] { TopMedium, TopRight, RightMedium, Center });
      //Image<Gray, Byte> BL_Img = CropImg(SRC_Img, new Point[] { LeftMedium, Center, BottomMedium, BottomLeft });
      //Image<Gray, Byte> BR_Img = CropImg(SRC_Img, new Point[] { Center, RightMedium, BottomRight, BottomMedium });
  
      return Mask;

    }
    #region LineBasedDetection Sub Functions

    private static VectorOfPoint FindMaxContour(Image<Gray, Byte> inputImg, Orientation dir)
    {
      // ищем контуры. Обязательно создаем дубликат изображения!
      Image<Gray, Byte> tmpImg = inputImg.Clone();
       
      VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
      Image<Gray, Byte> cannyImg = inputImg.Clone();
      CvInvoke.Canny(tmpImg, cannyImg, 50, 200);
      
      CvInvoke.FindContours(tmpImg, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

      //Фильтруем. Используем ориентацию, чтобы избежать ошибок
      Point Center = new Point(inputImg.Width / 2, inputImg.Height / 2);
      MCvPoint2D64f ContourCenter = new MCvPoint2D64f();
      MCvMoments moments = new MCvMoments();

      VectorOfVectorOfPoint contours2 = new VectorOfVectorOfPoint();
      contours2.Push(contours[79]);
      Image<Bgr, Byte> bgrimg = new Image<Bgr, byte>(@"C:\Users\Админ\Downloads\image63341262,2002.png");
      CvInvoke.DrawContours(bgrimg, contours2, -1, new MCvScalar(255, 0, 255));

      int index = 0;
      
      for (int i = 0; i < contours.Size; i++)
      {
        if (contours[i].Size > contours[index].Size)
        {
          moments = CvInvoke.Moments(contours[i]);
          ContourCenter = moments.GravityCenter;
          switch (dir)
          {
            case Orientation.Top:
              if (ContourCenter.Y > Center.Y) index = i; break;
            case Orientation.Bottom:
              if (ContourCenter.Y < Center.Y) index = i; break;
            case Orientation.Right:
              if (ContourCenter.X > Center.X) index = i; break;
            case Orientation.Left:
              if (ContourCenter.X < Center.X) index = i; break;
          }
        }
      }
      return contours[index];
    }

    private static Point[] FindExtremeContoursPoints(VectorOfVectorOfPoint contours)
    {
      int index = 0;
      VectorOfPoint curcontour = contours[0];

      for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].X < curcontour[index].X) index = i;
      Point TopLeft = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].X > curcontour[index].X) index = i;
      Point TopRight = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].Y < curcontour[index].Y) index = i;
      Point TopMedium = curcontour[index];
      
      curcontour = contours[2];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].X < curcontour[index].X) index = i;
      Point BottomLeft = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].X > curcontour[index].X) index = i;
      Point BottomRight = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].Y > curcontour[index].Y) index = i;
      Point BottomMedium = curcontour[index];
      
      curcontour = contours[3];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].Y < curcontour[index].Y) index = i;
      Point UpperLeft = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].Y > curcontour[index].Y) index = i;
      Point DownLeft = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].X < curcontour[index].X) index = i;
      Point LeftMedium = curcontour[index];
      
      curcontour = contours[1];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].Y < curcontour[index].Y) index = i;
      Point UpperRight = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].Y > curcontour[index].Y) index = i;
      Point DownRight = curcontour[index];
      index = 0; for (int i = 0; i < curcontour.Size; i++) if (curcontour[i].X > curcontour[index].X) index = i;
      Point RightMedium = curcontour[index];

      return new Point[] { TopLeft, TopMedium, TopRight,
                           UpperRight, RightMedium, DownRight, 
                           BottomRight, BottomMedium, BottomLeft,
                           DownLeft, LeftMedium, UpperLeft };
    }
    #endregion


    // эта функция точно работает. LineBasedDetection где то чуть-чуть отличается и потому не работает.
    public static Image<Gray, Byte> FindTable(Image<Gray, Byte> inputImg)
    {
      Image<Gray, Byte> smoothedImg = inputImg.SmoothMedian(3);
      CvInvoke.CLAHE(smoothedImg, 40, new System.Drawing.Size(16, 16), smoothedImg);
      CvInvoke.BilateralFilter(inputImg.SmoothMedian(3), smoothedImg, 50, 140, -1);

      float[,] TopKernelMatrix = new float[,] { { 0, -1, 0}, 
                                                 { 0,  0, 0}, 
                                                 { 0,  1, 0,}};
      float[,] BottomKernelMatrix = new float[,] { { 0, 1, 0}, 
                                                 { 0,  0, 0}, 
                                                 { 0,  -1, 0,}};
      float[,] LeftKernelMatrix = new float[,] { { 0,  0, 0}, 
                                                 { 1,  0, -1}, 
                                                 { 0,  0, 0,}};
      float[,] RightKernelMatrix = new float[,] { { 0, 0, 0}, 
                                                 { -1,  0, 1}, 
                                                 { 0,  0, 0,}};
      ConvolutionKernelF TopKernel = new ConvolutionKernelF(TopKernelMatrix);
      ConvolutionKernelF BottomKernel = new ConvolutionKernelF(BottomKernelMatrix);
      ConvolutionKernelF LeftKernel = new ConvolutionKernelF(LeftKernelMatrix);
      ConvolutionKernelF RightKernel = new ConvolutionKernelF(RightKernelMatrix);

      Image<Gray, Byte> TopImg = new Image<Gray, byte>(inputImg.Size);
      Image<Gray, Byte> BottomImg = new Image<Gray, byte>(inputImg.Size);
      Image<Gray, Byte> LeftImg = new Image<Gray, byte>(inputImg.Size);
      Image<Gray, Byte> RightImg = new Image<Gray, byte>(inputImg.Size);


      CvInvoke.Filter2D(smoothedImg, TopImg, TopKernel, new Point(1, 1));
      CvInvoke.Filter2D(smoothedImg, BottomImg, BottomKernel, new Point(1, 1));
      CvInvoke.Filter2D(smoothedImg, LeftImg, LeftKernel, new Point(1, 1));
      CvInvoke.Filter2D(smoothedImg, RightImg, RightKernel, new Point(1, 1));

      //TopImg = TopImg.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.GaussianC, ThresholdType.ToZero, 5, new Gray(0));

      TopImg = TopImg.ThresholdBinary(new Gray(15), new Gray(255));
      BottomImg = BottomImg.ThresholdBinary(new Gray(15), new Gray(255));
      LeftImg = LeftImg.ThresholdBinary(new Gray(15), new Gray(255));
      RightImg = RightImg.ThresholdBinary(new Gray(15), new Gray(255));

      VectorOfPoint TopContour = FindMaxContour(TopImg);
      VectorOfPoint BottomContour = FindMaxContour(BottomImg);
      VectorOfPoint LeftContour = FindMaxContour(LeftImg);
      VectorOfPoint RightContour = FindMaxContour(RightImg);

      VectorOfVectorOfPoint vectors = new VectorOfVectorOfPoint();
      vectors.Push(TopContour);
      vectors.Push(BottomContour);
      vectors.Push(LeftContour);
      vectors.Push(RightContour);

      //Image<Bgr, Byte> bgrimg = new Image<Bgr,byte>(@"C:\Users\Админ\Downloads\image63341262,2002.png");
      //CvInvoke.DrawContours(bgrimg, vectors, -1, new MCvScalar(0, 255, 0));

      int index = 0; for (int i = 0; i < TopContour.Size; i++) if (TopContour[i].X < TopContour[index].X) index = i;
      Point TopLeft = TopContour[index];
      index = 0; for (int i = 0; i < TopContour.Size; i++) if (TopContour[i].X > TopContour[index].X) index = i;
      Point TopRight = TopContour[index];
      index = 0; for (int i = 0; i < BottomContour.Size; i++) if (BottomContour[i].X < BottomContour[index].X) index = i;
      Point BottomLeft = BottomContour[index];
      index = 0; for (int i = 0; i < BottomContour.Size; i++) if (BottomContour[i].X > BottomContour[index].X) index = i;
      Point BottomRight = BottomContour[index];

      index = 0; for (int i = 0; i < BottomContour.Size; i++) if (TopContour[i].Y < TopContour[index].Y) index = i;
      Point TopMedium = TopContour[index];
      index = 0; for (int i = 0; i < BottomContour.Size; i++) if (BottomContour[i].Y < BottomContour[index].Y) index = i;
      Point BottomMedium = BottomContour[index];

      //ppp = ppp + BottomImg + LeftImg + RightImg;
      //CvInvoke.Line(bgrimg, TopLeft, BottomLeft, new MCvScalar(0, 0, 255));
      //CvInvoke.Line(bgrimg, TopRight, BottomRight, new MCvScalar(0, 0, 255));

      Image<Gray, Byte> Mask = inputImg.CopyBlank();
      VectorOfVectorOfPoint MaskVoVoP = new VectorOfVectorOfPoint();
      MaskVoVoP.Push(TopContour); MaskVoVoP.Push(BottomContour);
      CvInvoke.DrawContours(Mask, MaskVoVoP, -1, new MCvScalar(255), -1);
      CvInvoke.Line(Mask, TopLeft, BottomLeft, new MCvScalar(255));
      CvInvoke.Line(Mask, TopRight, BottomRight, new MCvScalar(255));

      VectorOfVectorOfPoint finalVector = new VectorOfVectorOfPoint();
      CvInvoke.FindContours(Mask, finalVector, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
      Mask = Mask.CopyBlank();
      CvInvoke.DrawContours(Mask, finalVector, -1, new MCvScalar(255), -1);

      return Mask;
    }
    #region 
    private static VectorOfPoint FindMaxContour(Image<Gray, Byte> inputImg)
    {
      Image<Gray, Byte> tmpImg = inputImg.Clone();
      VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
      CvInvoke.FindContours(tmpImg, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

      int index = 0;
      for (int i = 0; i < contours.Size; i++) if (contours[i].Size > contours[index].Size) index = i;
      return contours[index];
    }

    #endregion

    /*
    public static Image<Gray, Byte> SimpleMethodNum1(Image<Gray, Byte> inputImg)
    {
      Image<Gray, Byte> tmpImg = inputImg.SmoothMedian(5);
      tmpImg = tmpImg.ThresholdToZero( new Gray(30)); // так вырезается прямоугольник



      //CvInvoke.BilateralFilter(inputImg.SmoothMedian(3), smoothedImg, 50, 140, -1);
      //smoothedImg = smoothedImg.ThresholdToZero(new Gray(30));
      tmpImg = tmpImg.ThresholdBinary(new Gray(1), new Gray(255));
      VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
      
      Image<Bgr, Byte> sss = new Image<Bgr, byte>(inputImg.Bitmap);
      Image<Gray, Byte> cannyImg = inputImg.SmoothMedian(5);
      CvInvoke.Canny(tmpImg, cannyImg, 300, 250);
      //tmpImg = inputImg - tmpImg;
      //tmpImg = tmpImg.ThresholdToZero(new Gray(30)); // так вырезается прямоугольник
      CvInvoke.FindContours(cannyImg, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);

      VectorOfVectorOfPoint AA = new VectorOfVectorOfPoint();
      for (int i = 0; i < contours.Size; i++)
      {
        if (CvInvoke.ContourArea(contours[i]) >= 100) AA.Push(contours[i]);
      }

      for (int i = 0; i < AA.Size; i++)
      {
        VectorOfVectorOfPoint A = new VectorOfVectorOfPoint();
        A.Push(AA[i]);
        CvInvoke.DrawContours(sss, A, -1, new MCvScalar(255, 5 * i, 255));
      }
      return tmpImg;
    }
    */










  }
}
