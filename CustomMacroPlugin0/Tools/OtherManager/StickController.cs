﻿using CustomMacroBase.GamePadState;
using System;

namespace CustomMacroPlugin0.Tools.OtherManager
{
    //L:10 640 127 //R:10 160 127
    struct StickFixInfo
    {
        public int fix0_fix1_clip = 10;
        public double fix2_radius_max = 160;
        public double fix3_radius_max = 127;

        /// <summary>
        /// <para>_a：内圈死区&amp;反死区(0~127)</para>
        /// <para>_b：输出放大(128~640)</para>
        /// <para>_c：外圈死区(64~180)</para>
        /// </summary>
        public StickFixInfo(int _a, double _b, double _c = 127d)
        {
            fix0_fix1_clip = Math.Clamp(_a, 0, 127);
            fix2_radius_max = Math.Clamp(_b, 128, 640);
            fix3_radius_max = Math.Clamp(_c, 64, 180);
        }
    }

    sealed partial class StickController
    {
        StickFixInfo left_stick_fix_info, right_stick_fix_info;

        /// <summary>
        /// <para>_ls：左摇杆修正参数</para>
        /// <para>_rs：右摇杆修正参数</para>
        /// </summary>
        public StickController(StickFixInfo _ls, StickFixInfo _rs)
        {
            left_stick_fix_info = _ls;
            right_stick_fix_info = _rs;
        }

        public void ExecuteAction(in DS4StateLite _v) => RemoveUnwantedMovement(in _v);


        public void SetLeft(int _a, int _b, int _c)
        {
            SetDeadZoneLeft(_a);
            SetEnlargementFactorLeft(_b);
            SetClipRadiusLeft(_c);
        }
        public void SetRight(int _a, int _b, int _c)
        {
            SetDeadZoneRight(_a);
            SetEnlargementFactorRight(_b);
            SetClipRadiusRight(_c);
        }

        private void SetDeadZoneLeft(int _a) => left_stick_fix_info.fix0_fix1_clip = Math.Clamp(_a, 0, 127);
        private void SetEnlargementFactorLeft(int _b) => left_stick_fix_info.fix2_radius_max = Math.Clamp(_b, 128, 640);
        private void SetClipRadiusLeft(int _c) => left_stick_fix_info.fix3_radius_max = Math.Clamp(_c, 64, 180);
        private void SetDeadZoneRight(int _a) => right_stick_fix_info.fix0_fix1_clip = Math.Clamp(_a, 0, 127);
        private void SetEnlargementFactorRight(int _b) => right_stick_fix_info.fix2_radius_max = Math.Clamp(_b, 128, 640);
        private void SetClipRadiusRight(int _c) => right_stick_fix_info.fix3_radius_max = Math.Clamp(_c, 64, 180);
    }

    sealed partial class StickController
    {
        private void RemoveUnwantedMovement(in DS4StateLite _v)
        {
            LStickFix(_v, left_stick_fix_info);
            RStickFix(_v, right_stick_fix_info);
        }

        private void LStickFix(in DS4StateLite _v, StickFixInfo _) => fix_all(ref _v.LX, ref _v.LY, _.fix0_fix1_clip, _.fix2_radius_max, _.fix3_radius_max);
        private void RStickFix(in DS4StateLite _v, StickFixInfo _) => fix_all(ref _v.RX, ref _v.RY, _.fix0_fix1_clip, _.fix2_radius_max, _.fix3_radius_max);

        private void fix_all(ref byte x, ref byte y, int clip, double radius_2 = 128.0, double radius_3 = 127.0)
        {
            if (fix0(ref x, ref y, clip) is false)
            {
                fix1(ref x, ref y, clip);
                fix2(ref x, ref y, radius_2);
                fix3(ref x, ref y, radius_3);
            };
        }

        //死区（必要）
        private bool fix0(ref byte x, ref byte y, int clip)
        {
            if (Math.Pow(x - 128, 2.0) + Math.Pow(y - 128, 2.0) <= Math.Pow(clip, 2.0)) { x = y = 128; return true; }
            return false;
        }
        //反死区（必要）
        private void fix1(ref byte x, ref byte y, double clip)
        {
            int nx = x - 128;
            int ny = y - 128;
            double input = Math.Sqrt(Math.Pow(nx, 2.0) + Math.Pow(ny, 2.0));
            double ratio_reduce = (input - clip) / input;
            double ratio_magnify = 128.0 / (128.0 - clip);
            double ratio_final = ratio_reduce * ratio_magnify;
            x = (byte)Math.Clamp(128 + nx * ratio_final, 0, 255);
            y = (byte)Math.Clamp(128 + ny * ratio_final, 0, 255);
        }
        //将圆放大（必要） 128=0.61  160=0.65  192=0.68  224=0.71  256=0.73  288=0.75  320=0.76  352=0.77  384=0.79  416=0.8  640=0.85
        private void fix2(ref byte x, ref byte y, double radius_max)
        {
            int nx = x - 128;
            int ny = y - 128;
            double input = Math.Sqrt(Math.Pow(nx, 2.0) + Math.Pow(ny, 2.0));
            double ratio_limit = 128.0 / Math.Max(Math.Abs(nx), Math.Abs(ny));
            double ratio_target = 1.0 + (input / radius_max);
            double ratio_final = Math.Min(ratio_target, ratio_limit);
            x = (byte)Math.Clamp(128 + nx * ratio_final, 0, 255);
            y = (byte)Math.Clamp(128 + ny * ratio_final, 0, 255);
        }
        //从外至内裁剪圆（非必要） 
        private void fix3(ref byte x, ref byte y, double radius_max)
        {
            int nx = x - 128;
            int ny = y - 128;
            double input = Math.Sqrt(Math.Pow(nx, 2.0) + Math.Pow(ny, 2.0));
            double ratio_target = radius_max / input;
            double ratio_final = Math.Min(ratio_target, 1.0);
            x = (byte)Math.Clamp(128 + nx * ratio_final, 0, 255);
            y = (byte)Math.Clamp(128 + ny * ratio_final, 0, 255);
        }
    }
}
