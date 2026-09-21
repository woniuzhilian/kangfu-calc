using System;

namespace 抗浮计算书
{
    /// <summary>单建地下室整体抗浮计算的全部输入参数（与 UI 无关）。</summary>
    internal sealed class KangfuCans
    {
        public string dj = "";      //抗浮设计等级文本，
        public string cs = "";      //地下室层数文本，

        public double sw;      //抗浮设防水位标高，
        public double jcd;     //基础底板底标高，
        public float trz;      //混凝土容重，
        public float ydbg;     //-1层顶板顶标高，
        public float yftrz;    //-1层顶板覆土容重，
        public float dftrz;    //底板覆土容重，
        public float ydbh;     //-1层顶板厚，
        public float yfth;     //-1层顶板覆土厚，
        public float dfth;     //底板覆土厚，

        public float yxzlk, yyzlk, yxzlg, yyzlg, yxzls, yyzls;   //-1层x/y主梁宽、高、数量，
        public float yxclk, yyclk, yxclg, yyclg, yxcls, yycls;   //-1层x/y次梁宽、高、数量，
        public float yzc, yzk, yzs;                              //-1层柱长、宽、数量，

        public float jch, jcc, jck, jcs, jcbh;                   //基础厚、长、宽、数量、底板厚，
        public float qyx, qyy;                                   //验算区域x、y向长度，

        public float edbg, sdbg;                                 //-2/-3层顶板顶标高，
        public float eftrz, sftrz;                               //-2/-3层顶板覆土容重，
        public float edbh, sdbh;                                 //-2/-3层顶板厚，
        public float efth, sfth;                                 //-2/-3层顶板覆土厚，
        public float exzlk, eyzlk, exzlg, eyzlg, exzls, eyzls;   //-2层x/y主梁，
        public float sxzlk, syzlk, sxzlg, syzlg, sxzls, syzls;   //-3层x/y主梁，
        public float exclk, eyclk, exclg, eyclg, excls, eycls;   //-2层x/y次梁，
        public float sxclk, syclk, sxclg, syclg, sxcls, sycls;   //-3层x/y次梁，
        public float ezc, ezk, ezs;                              //-2层柱，
        public float szc, szk, szs;                              //-3层柱，

        //计算书"工程概况"原样回显的文本，
        public string swS = "", jcdS = "", ydbgS = "", yfthS = "", qyxS = "", qyyS = "", quyuS = "", mc = "";
    }

    /// <summary>计算结果；Youxiao=false 表示抗浮设计等级未选择。</summary>
    internal sealed class KangfuResult
    {
        public bool Youxiao = true;
        public string Bizhi = "";
        public string Jielun = "";
        public string JisuanShu = "";
    }

    /// <summary>整体抗浮验算与计算书生成，纯计算、无 UI 依赖。</summary>
    internal static class KangfuJisuan
    {
        public const float GammaWater = 10f;          //水的重度，

        public static KangfuResult Jisuan(KangfuCans p)
        {
            string dj = p.dj, cs = p.cs;
            double sw = p.sw, jcd = p.jcd;
            float trz = p.trz, ydbg = p.ydbg, yftrz = p.yftrz, dftrz = p.dftrz;
            float ydbh = p.ydbh, yfth = p.yfth, dfth = p.dfth;
            float yxzlk = p.yxzlk, yyzlk = p.yyzlk, yxzlg = p.yxzlg, yyzlg = p.yyzlg, yxzls = p.yxzls, yyzls = p.yyzls;
            float yxclk = p.yxclk, yyclk = p.yyclk, yxclg = p.yxclg, yyclg = p.yyclg, yxcls = p.yxcls, yycls = p.yycls;
            float yzc = p.yzc, yzk = p.yzk, yzs = p.yzs;
            float jch = p.jch, jcc = p.jcc, jck = p.jck, jcs = p.jcs, jcbh = p.jcbh;
            float qyx = p.qyx, qyy = p.qyy;
            float edbg = p.edbg, sdbg = p.sdbg, eftrz = p.eftrz, sftrz = p.sftrz;
            float edbh = p.edbh, sdbh = p.sdbh, efth = p.efth, sfth = p.sfth;
            float exzlk = p.exzlk, eyzlk = p.eyzlk, exzlg = p.exzlg, eyzlg = p.eyzlg, exzls = p.exzls, eyzls = p.eyzls;
            float sxzlk = p.sxzlk, syzlk = p.syzlk, sxzlg = p.sxzlg, syzlg = p.syzlg, sxzls = p.sxzls, syzls = p.syzls;
            float exclk = p.exclk, eyclk = p.eyclk, exclg = p.exclg, eyclg = p.eyclg, excls = p.excls, eycls = p.eycls;
            float sxclk = p.sxclk, syclk = p.syclk, sxclg = p.sxclg, syclg = p.syclg, sxcls = p.sxcls, sycls = p.sycls;
            float ezc = p.ezc, ezk = p.ezk, ezs = p.ezs;
            float szc = p.szc, szk = p.szk, szs = p.szs;

            float mj = qyx * qyy;                                 //验算区域面积
            double xs;                                                  //抗浮稳定系数，
            double yzdg = 0;                                             //-1层柱底标高，
            double ezdg = 0;                                             //-2层柱底标高，
            double kfxs;                                              //填筑体抗浮力组合系数，
            double zzkfxs;                                            //结构自重抗浮力组合系数，
            if (cs == "1层")
            {
                yzdg = jcd + jcbh;        //-1层柱底标高=基础底板底标高+基础板厚，
            }
            else if (cs == "2层")
            {
                yzdg = edbg;              //-1层柱底标高=-2层顶板顶标高，
                ezdg = jcd + jcbh;        //-2层柱底标高=基础底板底标高+基础板厚，
            }
            else if (cs == "3层")
            {
                yzdg = edbg;              //-1层柱底标高=-2层顶板顶标高，
                ezdg = sdbg;              //-2层柱底标高=-3层顶板顶标高，
            }

            if (dj == "甲级")
            {
                xs = 1.1;
                kfxs = 0.9;
                zzkfxs = 1.0;
            }
            else if (dj == "乙级")
            {
                xs = 1.05;
                kfxs = 0.9;
                zzkfxs = 1.0;
            }
            else if (dj == "丙级")
            {
                xs = 1.05;
                kfxs = 0.95;
                zzkfxs = 1.05;
            }
            else
            {
                return new KangfuResult { Youxiao = false };
            }

            double yftg = yfth * yftrz * mj * kfxs;                            //-1层顶板覆土自重，
            double eftg = efth * eftrz * mj * kfxs;                            //-2层顶板覆土自重，
            double sftg = sfth * sftrz * mj * kfxs;                            //-3层顶板覆土自重，

            double ybg = ydbh * trz * mj * zzkfxs;                            //-1层顶板自重，
            double ebg = edbh * trz * mj * zzkfxs;                            //-2层顶板自重，
            double sbg = sdbh * trz * mj * zzkfxs;                            //-3层顶板自重，

            double yzlg = yxzlk * (yxzlg - ydbh) * qyx * yxzls * trz * zzkfxs + yyzlk * (yyzlg - ydbh) * qyy * yyzls * trz * zzkfxs;                            //-1层主梁自重，
            double ezlg = exzlk * (exzlg - edbh) * qyx * exzls * trz * zzkfxs + eyzlk * (eyzlg - edbh) * qyy * eyzls * trz * zzkfxs;                            //-2层主梁自重，
            double szlg = sxzlk * (sxzlg - sdbh) * qyx * sxzls * trz * zzkfxs + syzlk * (syzlg - sdbh) * qyy * syzls * trz * zzkfxs;                            //-3层主梁自重，

            double yclg = yxclk * (yxclg - ydbh) * qyx * yxcls * trz * zzkfxs + yyclk * (yyclg - ydbh) * qyy * yycls * trz * zzkfxs;                            //-1层次梁自重，
            double eclg = exclk * (exclg - edbh) * qyx * excls * trz * zzkfxs + eyclk * (eyclg - edbh) * qyy * eycls * trz * zzkfxs;                            //-2层次梁自重，
            double sclg = sxclk * (sxclg - sdbh) * qyx * sxcls * trz * zzkfxs + syclk * (syclg - sdbh) * qyy * sycls * trz * zzkfxs;                            //-3层次梁自重，

            double yzg = yzc * yzk * yzs * trz * (ydbg - yzdg - ((yxzlg + yyzlg) / 2)) * zzkfxs;                            //-1层柱子自重，
            double ezg = ezc * ezk * ezs * trz * (edbg - ezdg - ((exzlg + eyzlg) / 2)) * zzkfxs;                            //-2层柱子自重，
            double szg = szc * szk * szs * trz * (sdbg - jcd - jcbh - ((sxzlg + syzlg) / 2)) * zzkfxs;                            //-3层柱子自重，

            double jcg = jcc * jck * (jch - jcbh) * (trz - GammaWater) * jcs * zzkfxs;                            //基础自重，已扣除水浮力
            double jcbg = jcbh * trz * mj * zzkfxs;                                             //基础底板自重，
            double dftg = dfth * dftrz * mj * kfxs * kfxs;                                           //基础底板覆土自重，
            double yg = yftg + ybg + yzlg + yclg + yzg;                                //-1层构件总自重，
            double eg = eftg + ebg + ezlg + eclg + ezg;                                //-2层构件总自重，
            double sg = sftg + sbg + szlg + sclg + szg;                                //-3层构件总自重，

            switch (cs)
            {
                case "1层":
                    eg = 0;                   //-2层自重等于0，
                    sg = 0;                   //-3层自重等于0，
                    break;
                case "2层":
                    sg = 0;                   //-3层自重等于0，
                    break;
                case "3层":
                    break;
            }
            double sfl = (sw - jcd) * GammaWater * qyx * qyy;                                             //总水浮力，
            double sflps = sfl / mj;                                          //单位面积水浮力，
            double zizhong = yg + eg + sg + jcg + jcbg + dftg;                        //构件总自重，
            double zzps = zizhong / mj;                                          //单位面积构件自重，
            double bz = zzps / sflps;                                  //构件自重与水浮力的比值
            string fuhao;
            string zongjie;
            string jielun;
            if (bz >= xs)
            {
                jielun = "满足抗浮要求！";
                fuhao = ">";
                zongjie = "整体抗浮验算满足规范要求。" + "\r\n" + "因整个地下室是该典型的区格形式构成，故只要该区格局部抗浮验算满足，则地下室在考虑底板外挑和墙体重量后的整体抗浮也能满足抗浮要求。";
            }
            else
            {
                jielun = "抗浮不满足要求！";
                fuhao = "<";
                zongjie = "整体抗浮验算不满足规范要求！";
            }

            string bizhi = bz.ToString("F4");
            string zongjie1 = "-1层顶板覆土产生的抗浮力标准值=" + yfth + "*" + yftrz + "*" + qyx + "*" + qyy + "*" + kfxs + "=" + yftg.ToString("F2") + "kN" + "\r\n" +
                "-1层顶板自重产生的抗浮力标准值=" + ydbh + "*" + trz + "*" + qyx + "*" + qyy + "*" + zzkfxs + "=" + ybg.ToString("F2") + "kN" + "\r\n" +
                "-1层顶板主梁产生的抗浮力标准值=" + yxzlk + "*(" + yxzlg + "-" + ydbh + ")*" + qyx + "*" + yxzls + "*" + trz + "*" + zzkfxs + "+" + yyzlk + "*(" + yyzlg + "-" + ydbh + ")*" + qyy + "*" + yyzls + "*" + trz + "*" + zzkfxs + "= " + yzlg.ToString("F2") + "kN" + "\r\n" +
                "-1层顶板次梁产生的抗浮力标准值=" + yxclk + "*(" + yxclg + "-" + ydbh + ")*" + qyx + "*" + yxcls + "*" + trz + "*" + zzkfxs + "+" + yyclk + "*(" + yyclg + "-" + ydbh + ")*" + qyy + "*" + yycls + "*" + trz + "*" + zzkfxs + "= " + yclg.ToString("F2") + "kN" + "\r\n" +
                "-1层柱子产生的抗浮力标准值=" + yzc + "*" + yzk + "*" + yzs + "*" + trz + "*{(" + ydbg + ")" + "-(" + yzdg.ToString("F2") + ")-[(" + yxzlg + "+" + yyzlg + ")/" + "2]}" + "*" + zzkfxs + "= " + yzg.ToString("F2") + "kN" + "\r\n";
            string zongjie2 = "-2层顶板覆土产生的抗浮力标准值=" + efth + "*" + eftrz + "*" + qyx + "*" + qyy + "*" + kfxs + "=" + eftg.ToString("F2") + "kN" + "\r\n" +
               "-2层顶板自重产生的抗浮力标准值=" + edbh + "*" + trz + "*" + qyx + "*" + qyy + "*" + zzkfxs + "=" + ebg.ToString("F2") + "kN" + "\r\n" +
               "-2层顶板主梁产生的抗浮力标准值=" + exzlk + "*(" + exzlg + "-" + edbh + ")*" + qyx + "*" + exzls + "*" + trz + "*" + zzkfxs + "+" + eyzlk + "*(" + eyzlg + "-" + edbh + ")*" + qyy + "*" + eyzls + "*" + trz + "*" + zzkfxs + "= " + ezlg.ToString("F2") + "kN" + "\r\n" +
               "-2层顶板次梁产生的抗浮力标准值=" + exclk + "*(" + exclg + "-" + edbh + ")*" + qyx + "*" + excls + "*" + trz + "*" + zzkfxs + "+" + eyclk + "*(" + eyclg + "-" + edbh + ")*" + qyy + "*" + eycls + "*" + trz + "*" + zzkfxs + "= " + eclg.ToString("F2") + "kN" + "\r\n" +
               "-2层柱子产生的抗浮力标准值=" + ezc + "*" + ezk + "*" + ezs + "*" + trz + "*{(" + edbg + ")" + "-(" + ezdg.ToString("F2") + ")-[(" + exzlg + "+" + eyzlg + ")/" + "2]}" + "*" + zzkfxs + "= " + ezg.ToString("F2") + "kN" + "\r\n";
            string zongjie3 = "-3层顶板覆土产生的抗浮力标准值=" + sfth + "*" + sftrz + "*" + qyx + "*" + qyy + "*" + kfxs + "=" + sftg.ToString("F2") + "kN" + "\r\n" +
              "-3层顶板自重产生的抗浮力标准值=" + sdbh + "*" + trz + "*" + qyx + "*" + qyy + "*" + zzkfxs + "=" + sbg.ToString("F2") + "kN" + "\r\n" +
              "-3层顶板主梁产生的抗浮力标准值=" + sxzlk + "*(" + sxzlg + "-" + sdbh + ")*" + qyx + "*" + sxzls + "*" + trz + "*" + zzkfxs + "+" + syzlk + "*(" + syzlg + "-" + sdbh + ")*" + qyy + "*" + syzls + "*" + trz + "*" + zzkfxs + "= " + szlg.ToString("F2") + "kN" + "\r\n" +
              "-3层顶板次梁产生的抗浮力标准值=" + sxclk + "*(" + sxclg + "-" + sdbh + ")*" + qyx + "*" + sxcls + "*" + trz + "*" + zzkfxs + "+" + syclk + "*(" + syclg + "-" + sdbh + ")*" + qyy + "*" + sycls + "*" + trz + "*" + zzkfxs + "= " + sclg.ToString("F2") + "kN" + "\r\n" +
              "-3层柱子产生的抗浮力标准值=" + szc + "*" + szk + "*" + szs + "*" + trz + "*{(" + sdbg + ")-(" + jcd + ")-" + jcbh + "-[(" + sxzlg + "+" + syzlg + ")/" + "2]}" + "*" + zzkfxs + "= " + szg.ToString("F2") + "kN" + "\r\n";

            switch (cs)
            {
                case "1层":
                    zongjie2 = "";                    //-2层自重清空，
                    zongjie3 = "";                    //-3层自重清空，
                    break;
                case "2层":
                    zongjie3 = "";                    //-3层自重清空，
                    break;
            }
            string zongzizhong;
            if (cs == "1层")
            {
                zongzizhong = "结构自重产生的总抗浮力标准值=" + yg.ToString("F2") + "+" + jcg.ToString("F2") + "+" + jcbg.ToString("F2") + "+" + dftg.ToString("F2") + "=" + zizhong.ToString("F2") + "kN";
            }
            else if (cs == "2层")
            {
                zongzizhong = "结构自重产生的总抗浮力标准值=" + yg.ToString("F2") + "+" + eg.ToString("F2") + "+" + jcg.ToString("F2") + "+" + jcbg.ToString("F2") + "+" + dftg.ToString("F2") + "=" + zizhong.ToString("F2") + "kN";
            }
            else
            {
                zongzizhong = "结构自重产生的总抗浮力标准值=" + yg.ToString("F2") + "+" + eg.ToString("F2") + "+" + sg.ToString("F2") + "+" + jcg.ToString("F2") + "+" + jcbg.ToString("F2") + "+" + dftg.ToString("F2") + "=" + zizhong.ToString("F2") + "kN";
            }

            string jss;

            jss = "************************单建地下室整体抗浮计算书************************" + "\r\n" +
                "一、工程概况" + "\r\n" +
                "工程名称：" + p.mc + "\r\n" +
                "-1层顶板顶标高：" + p.ydbgS + "m" + "\r\n" +
                "抗浮设防水位标高：" + p.swS + "m" + "\r\n" +
                "抗浮工程设计等级：" + dj + "\r\n" +
                "地下室层数：" + cs + "\r\n" +
                "基础底板底标高：" + p.jcdS + "m" + "\r\n" +
                "-1层顶板覆土厚度：" + p.yfthS + "m" + "\r\n" +
                "抗浮力组合系数（结构自重）：" + zzkfxs.ToString("F2") + "\r\n" +
                "抗浮力组合系数（填筑体）：" + kfxs + "\r\n" +
                "抗浮稳定安全系数最小值：" + xs + "\r\n" +
                "二、规范依据" + "\r\n" +
                "《工程结构通用规范》GB55001-2021" + "\r\n" +
                "《建筑与市政地基基础通用规范》GB55003-2021" + "\r\n" +
                "《建筑结构可靠性设计统一标准》GB50068-2018" + "\r\n" +
                "《建筑工程抗浮技术标准》JGJ475-2019" + "\r\n" +
                "《建筑地基基础设计规范》GB50007-2011" + "\r\n" +
                "《建筑结构荷载规范》GB50009-2012" + "\r\n" +
                "三、抗浮验算" + "\r\n" +
                "取" + p.quyuS + "所围成的区域作为典型区格进行整体抗浮验算，该区域长度为：" + p.qyxS + "m，宽度为：" + p.qyyS + "m" + "\r\n" +
                zongjie1 + zongjie2 + zongjie3 +
                "基础底板覆土自重产生的抗浮力标准值=" + dfth + "*" + dftrz + "*" + qyx + "*" + qyy + "*" + kfxs + "*" + kfxs + "=" + dftg.ToString("F2") + "kN" + "\r\n" +
                "基础底板自重产生的抗浮力标准值=" + jcbh + "*" + trz + "*" + qyx + "*" + qyy + "*" + zzkfxs + "=" + jcbg.ToString("F2") + "kN" + "\r\n" +
                "基础自重产生的抗浮力标准值=" + jcc + "*" + jck + "*(" + jch + "-" + jcbh + ")*(" + trz + "-" + GammaWater + ")*" + jcs + "*" + zzkfxs + "=" + jcg.ToString("F2") + "kN" + "\r\n" +
                zongzizhong + "\r\n" +
                "结构构件自重产生的单位面积抗浮力标准值=" + zizhong.ToString("F2") + "/" + qyx + "/" + qyy + "=" + zzps.ToString("F2") + "kN" + "\r\n" +
                "水浮力总作用值=" + "[(" + sw.ToString("F2") + ")" + "-(" + jcd.ToString("F2") + ")]*" + GammaWater + "*" + qyx + "*" + qyy + "=" + sfl.ToString("F2") + "kN" + "\r\n" +
                "单位面积水浮力作用值=" + sfl.ToString("F2") + "/" + qyx + "/" + qyy + "=" + sflps.ToString("F2") + "kN" + "\r\n" +
                "抗浮稳定安全系数=" + zzps.ToString("F2") + "/" + sflps.ToString("F2") + "=" + bz.ToString("F4") + "\r\n" +
                "四、结论" + "\r\n" +
                "根据《建筑地基基础设计规范》（GB50007-2011）第5.4.3条规定，" + bz.ToString("F4") + fuhao + xs + "。" + zongjie;

            if (jcd >= sw)                                                     //基础底板底高于抗浮水位时，无需进行抗浮验算
            {
                jss = "抗浮水位低于基础底板底，无需进行抗浮验算！";
                jielun = "";
                bizhi = "";
            }

            return new KangfuResult { Youxiao = true, Bizhi = bizhi, Jielun = jielun, JisuanShu = jss };
        }
    }
}
