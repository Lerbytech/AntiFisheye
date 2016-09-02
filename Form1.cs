using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
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

  public partial class Form1 : Form
  {
    Image<Gray, Byte> SRC_Img;
    Image<Gray, Byte> Corrected_Img;
    public Form1()
    {
      InitializeComponent();
      SRC_Img = new Image<Gray, byte>(@"C:\Users\Админ\Downloads\image63341262,2002.png");

      FishEye FE = new FishEye();
      FE.CalculateParameters(SRC_Img, 325);
      Image<Gray, Byte> result = SRC_Img.Clone();

      Image<Gray, Byte> Mask;
      // метода для маски
      Mask = BarTableDetector.FindTable(SRC_Img);

      // теперь по маске получим нужный кроп изображения
      Image<Gray, Byte> Masked_Img = SRC_Img.Copy(Mask);
      Corrected_Img = FE.RemoveFisheye(Masked_Img);


      //Stopwatch timer = new Stopwatch();
      //timer.Start();
      //for (int i = 0; i < 500; i++)
      //{
//        FE.CalculateParameters(SRC_Img, 310);
        //Corrected_Img = FE.RemoveFisheye(Masked_Img, 0);
      //}
      //result = FE.Test(SRC_Img, 325);
      //timer.Stop();
      //long l = timer.ElapsedMilliseconds / 500;
    }



    public void MainStuff()
    {
      SRC_Img = new Image<Gray, byte>(@"C:\Users\Админ\Downloads\image63341262,2002.png");
      Corrected_Img = SRC_Img.Clone();

      //CvInvoke.CLAHE(SRC_Img, 40, new Size(8, 8), Corrected_Img);
      //CvInvoke.FindChessboardCorners(SRC_Img, new Size(8,8), vec);
#region 
      PointF[] corners = new PointF[] { new PointF( 100, 196),  new PointF( 261, 190), new PointF( 417, 192),  new PointF( 584, 201),
                                      new PointF( 111, 277),  new PointF( 284, 287), new PointF( 458, 291),  new PointF( 580, 284),
                                      new PointF( 130, 368), new PointF( 276, 395), new PointF( 429, 391),  new PointF( 563, 365)
                                     };
#endregion
      VectorOfPointF vec = new VectorOfPointF();
      vec.Push(corners);
      // X: 0 - 480 / 3 ||0 159 329 479
      // Y: 0 - 210 / 2 || 0 104 209

      MCvPoint3D32f[] objCorners = new MCvPoint3D32f[] { new MCvPoint3D32f( 0, 0, 0.0f),    new MCvPoint3D32f(SRC_Img.Width / 3 - 1, 0, 0.0f),       new MCvPoint3D32f( 2 * SRC_Img.Width / 3 - 1, 0, 0.0f),    new MCvPoint3D32f( SRC_Img.Width - 1, 0, 0.0f),
                                           new MCvPoint3D32f( 0, SRC_Img.Height / 2 - 1, 0.0f),  new MCvPoint3D32f(SRC_Img.Width / 3 - 1, SRC_Img.Height / 2 - 1, 0.0f),     new MCvPoint3D32f( 2 * SRC_Img.Width / 3 - 1, SRC_Img.Height / 2 - 1, 0.0f),  new MCvPoint3D32f( SRC_Img.Width - 1, SRC_Img.Height / 2 - 1, 0.0f),
                                           new MCvPoint3D32f( 0, SRC_Img.Height - 1, 0.0f),  new MCvPoint3D32f( SRC_Img.Width / 3 - 1, SRC_Img.Height - 1, 0.0f),    new MCvPoint3D32f( 2 * SRC_Img.Width / 3 - 1, SRC_Img.Height - 1, 0.0f),  new MCvPoint3D32f( SRC_Img.Width - 1, SRC_Img.Height - 1, 0.0f)
                                     };
      /*
      for (int i = 0; i < objCorners.Length; i++)
      {
        objCorners[i].X += SRC_Img.Width / 2;
        objCorners[i].Y += SRC_Img.Height / 2;
      }*/
      //VectorOfPointF objvec = new VectorOfPointF();
      //objvec.Push(objCorners);

      
      //Corrected_Img = FindTable(SRC_Img);
      Matrix<double> CameraMatrix = new Matrix<double>(3, 3, 1);
      CameraMatrix[0, 0] = 1;
      CameraMatrix[1, 1] = 1;
      CameraMatrix[2, 2] = 1;
      CameraMatrix[0, 2] = 349.417;
      CameraMatrix[1, 2] = 286.417;

      Mat newCameraMatrix = CvInvoke.GetDefaultNewCameraMatrix(CameraMatrix);
      //CvInvoke.Undistort(SRC_Img, Corrected_Img,  
      //CvInvoke.FindChessboardCorners(SRC_Img, new System.Drawing.Size(5,5), 

      Mat distCoeffs = new Mat(1, 5, DepthType.Cv32F, 1);
      Mat rotCoeffs = new Mat();
      Mat translVectors = new Mat();
      MCvTermCriteria TermCriteria = new MCvTermCriteria(30, 0.1);
      Corrected_Img = SRC_Img.Clone();
      CvInvoke.DrawChessboardCorners(Corrected_Img, new System.Drawing.Size(4, 3), vec, true);
      //CvInvoke.CornerSubPix(SRC_Img, vec, new Size(2, 2), new Size(-1, -1), TermCriteria);
      //CvInvoke.DrawChessboardCorners(SRC_Img, new System.Drawing.Size(4, 3), objvec, true);
      /*
      try
      {
        CvInvoke.Remap(SRC_Img, Corrected_Img, vec, objvec, Inter.Nearest, BorderType.Constant);
      } catch (Exception ex) { string s = ex.Message; }
      */
      VectorOfPoint3D32F obj3dvec = new VectorOfPoint3D32F();
      obj3dvec.Push(objCorners);

      try
      {
        MCvPoint3D32f[][] corners_object_list = new MCvPoint3D32f[1][];
        PointF[][] corners_points_list = new PointF[1][];
        corners_object_list[0] = objCorners;
        corners_points_list[0] = corners;
        double r = CvInvoke.CalibrateCamera(obj3dvec,
                                            vec,
                                            SRC_Img.Size,
                                            CameraMatrix,
                                            distCoeffs, 
                                            rotCoeffs,
                                            translVectors,
                                            CalibType.Default, 
                                            TermCriteria);
       
        //double error = CameraCalibration.CalibrateCamera(corners_object_list, corners_points_list, Gray_Frame.Size, IC, Emgu.CV.CvEnum.CALIB_TYPE.CV_CALIB_RATIONAL_MODEL, out EX_Param);
        r += 0;
        //Matrix<float> dist = new Matrix<float>( new float[] { 

        //CvInvoke.Undistort(SRC_Img, Corrected_Img, cameraMatrix, );
      } catch (Exception ex) { }

      IntrinsicCameraParameters IC = new IntrinsicCameraParameters(8);
      Matrix<float> Map1, Map2;
      IC.InitUndistortMap(SRC_Img.Width, SRC_Img.Height, out Map1, out Map2);
      Image<Gray, Byte> stuff = Undistort(SRC_Img);

      imageBox1.Image = SRC_Img.Resize(imageBox1.Width, imageBox1.Height, Inter.Linear);
      imageBox2.Image = Corrected_Img.Resize(imageBox1.Width, imageBox1.Height, Inter.Linear);




    }

    public void SuperR()
    {
      SRC_Img = new Image<Gray, byte>(@"C:\Users\Админ\Downloads\image63341262,2002.png");
      Corrected_Img = SRC_Img.Clone();

      PointF[] corners = new PointF[] { new PointF( 100, 196),  new PointF( 261, 190), new PointF( 417, 192),  new PointF( 584, 201),
                                      new PointF( 111, 277),  new PointF( 284, 287), new PointF( 458, 291),  new PointF( 580, 284),
                                      new PointF( 130, 368), new PointF( 276, 395), new PointF( 429, 391),  new PointF( 563, 365)
                                     };
       /*MCvPoint3D32f[] objCorners = new MCvPoint3D32f[] { new MCvPoint3D32f( 0, 0, 0.0f),    new MCvPoint3D32f(SRC_Img.Width / 3 - 1, 0, 0.0f),       new MCvPoint3D32f( 2 * SRC_Img.Width / 3 - 1, 0, 0.0f),    new MCvPoint3D32f( SRC_Img.Width - 1, 0, 0.0f),
                                           new MCvPoint3D32f( 0, SRC_Img.Height / 2 - 1, 0.0f),  new MCvPoint3D32f(SRC_Img.Width / 3 - 1, SRC_Img.Height / 2 - 1, 0.0f),     new MCvPoint3D32f( 2 * SRC_Img.Width / 3 - 1, SRC_Img.Height / 2 - 1, 0.0f),  new MCvPoint3D32f( SRC_Img.Width - 1, SRC_Img.Height / 2 - 1, 0.0f),
                                           new MCvPoint3D32f( 0, SRC_Img.Height - 1, 0.0f),  new MCvPoint3D32f( SRC_Img.Width / 3 - 1, SRC_Img.Height - 1, 0.0f),    new MCvPoint3D32f( 2 * SRC_Img.Width / 3 - 1, SRC_Img.Height - 1, 0.0f),  new MCvPoint3D32f( SRC_Img.Width - 1, SRC_Img.Height - 1, 0.0f)
                                     };
      */
       // X: 0 - 480 / 3 ||0 159 329 479
       // Y: 0 - 210 / 2 || 0 104 209
      
       MCvPoint3D32f[] objCorners = new MCvPoint3D32f[] { new MCvPoint3D32f( 0, 0, 0.0f),    new MCvPoint3D32f(159, 0, 0.0f),    new MCvPoint3D32f(329 , 0, 0.0f),    new MCvPoint3D32f(479 , 0, 0.0f),
                                                          new MCvPoint3D32f( 0, 104, 0.0f),  new MCvPoint3D32f(159, 104, 0.0f),     new MCvPoint3D32f(329 ,104, 0.0f),  new MCvPoint3D32f(479,104, 0.0f),
                                                          new MCvPoint3D32f( 0, 209, 0.0f),  new MCvPoint3D32f(159,209 , 0.0f),    new MCvPoint3D32f(329 , 209, 0.0f),  new MCvPoint3D32f(479 ,209, 0.0f)
                                     };
      
      VectorOfPointF veccorners = new VectorOfPointF();
      veccorners.Push(corners);
      VectorOfPoint3D32F vecobjcorners = new VectorOfPoint3D32F();
      vecobjcorners.Push(objCorners);

      MCvTermCriteria TermCriteria = new MCvTermCriteria(30, 0.1);
      CvInvoke.CornerSubPix(SRC_Img, veccorners, new Size(2, 2), new Size(-1, -1), TermCriteria);

      IntrinsicCameraParameters intrisic = new IntrinsicCameraParameters();
      ExtrinsicCameraParameters[] extrinsic;
      intrisic.IntrinsicMatrix = new Matrix<double>( new double[,] { {1,0,349.417}, {0,1,286.417}, {0,0,1} } );
      try
      {
        Matrix<float> distortCoeffs = new Matrix<float>(1, 4);
        Mat rotationVectors = new Mat();
        //rotationVectors[0] = new Mat(3,1, DepthType.Cv32F, 1);
        Mat translationVectors = new Mat();
        //translationVectors[0] = new Mat(1, 3, DepthType.Cv32F, 1);
        /*
        double error = CvInvoke.CalibrateCamera(new MCvPoint3D32f[][] { objCorners }, new PointF[][] { veccorners.ToArray() },
             SRC_Img.Size, intrisic.IntrinsicMatrix, distortCoeffs, CalibType.UserIntrinsicGuess, new MCvTermCriteria(30, 0.01), out rotationVectors, out translationVectors);
        */
        /*
        
        Fisheye.Calibrate(vecobjcorners, veccorners, SRC_Img.Size, intrisic.IntrinsicMatrix, distortCoeffs, rotationVectors, translationVectors, 
          Fisheye.CalibrationFlag.UseIntrinsicGuess, TermCriteria);
         * */

        Matrix<float> matrix = new Matrix<float>( new float[,] { {1,0,349}, {0,1,286}, {0,0,1} } );
        Fisheye.UndistorImage(SRC_Img, Corrected_Img, matrix, new VectorOfFloat(new float[] { 3500, 3500, 0, 0 }));
        Image<Gray, Byte> Res_Img = new Image<Gray, byte>(2 * SRC_Img.Width, SRC_Img.Height);
        CvInvoke.HConcat(SRC_Img, Corrected_Img, Res_Img);
        int error = 0;
        error++;
        //error += 0;
        //Array aa = rotationVectors[0].Data;
        //error += 0;
        //float q = rotationVectors.ElementAt<float>(0);

      }
      catch (Exception) { }
    }

    /*
    private Image<Gray, Byte> CropImage(Image<Gray, Byte> inputImg, Image<Gray, Byte> Mask)
    {
      VectorOfVectorOfPoint Mask = new 
      CvInvoke.
    }*/

    public Image<Gray, Byte> Undistort(Image<Gray, Byte> inputImg)
    {
      PointF[] corners = new PointF[] { new PointF( 100, 196),  new PointF( 261, 190), new PointF( 417, 192),  new PointF( 584, 201),
                                      new PointF( 111, 277),  new PointF( 284, 287), new PointF( 458, 291),  new PointF( 580, 284),
                                      new PointF( 130, 368), new PointF( 276, 395), new PointF( 429, 391),  new PointF( 563, 365)
                                     };

      double cX = 0;
      double cY = 0;
      for (int i = 0; i < corners.Length; i++)
      {
        cX += corners[i].X;
        cY += corners[i].Y;
      }
      cX /= corners.Length; // 349.417
      cY /= corners.Length;//286.417

      //VectorOfPointF v = new VectorOfPointF();
      //v.Push(corners);
      //v.Push(new PointF[] {new PointF(cX, cY) });
      
      Image<Gray, Byte> resImg = new Image<Gray, byte>(inputImg.Size);

      double zoom = 1;
      double strength = 0.25f;
      double corrRadius = Math.Sqrt(inputImg.Width * inputImg.Width + inputImg.Height * inputImg.Height) / strength;
      double newX = 0;
      double newY = 0;
      double distance = 0;
      double r = 0;
      double theta = 0;

      for (int i = 0; i < inputImg.Width; i++)
        for (int j = 0; j < inputImg.Height; j++)
        {
          newX = i - cX;
          newY = j - cY;
          distance = Math.Sqrt(newX * newX + newY * newY);
          r = distance / corrRadius;
          theta = Math.Atan(r) / r;

          newX = (int)Math.Round(cX + theta * newX * zoom);
          newY = (int)Math.Round(cY + theta * newY * zoom);

          resImg[(int)newY, (int)newX] = inputImg[j, i];
        }


      return null;
    }
  }
}

