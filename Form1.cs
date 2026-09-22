using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace 抗浮计算书
{
    public partial class zhchuangkou : Form
    {
        public zhchuangkou()
        {
            InitializeComponent();
            MakeJisuanshuViewOnly();
        }

        // 计算书展示框：只读、不可选中、不可复制，仅供查看。
        private void MakeJisuanshuViewOnly()
        {
            jisuanshu.TabStop = false;
            jisuanshu.ContextMenuStrip = new ContextMenuStrip(); // 空白菜单，去掉右键"复制"
            jisuanshu.GotFocus += (s, e) => jisuanshu.SelectionLength = 0;
            jisuanshu.MouseDown += (s, e) => jisuanshu.SelectionLength = 0;
            jisuanshu.MouseMove += (s, e) => { if (e.Button != MouseButtons.None) jisuanshu.SelectionLength = 0; };
            jisuanshu.MouseUp += (s, e) => jisuanshu.SelectionLength = 0;
            jisuanshu.KeyDown += Jisuanshu_KeyDown;
        }

        private void Jisuanshu_KeyDown(object? sender, KeyEventArgs e)
        {
            // 拦截 全选 / 复制 / 剪切 相关组合键
            bool ctrl = (e.Control || (ModifierKeys & Keys.Control) == Keys.Control);
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
            if (e.KeyCode == Keys.C && ctrl) e.SuppressKeyPress = true;
            else if (e.KeyCode == Keys.A && ctrl) e.SuppressKeyPress = true;
            else if (e.KeyCode == Keys.X && ctrl) e.SuppressKeyPress = true;
            else if (e.KeyCode == Keys.Insert && (ctrl || shift)) e.SuppressKeyPress = true;
            else if (e.KeyCode == Keys.C && shift) e.SuppressKeyPress = true;
            else if (shift && (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                               e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                               e.KeyCode == Keys.Home || e.KeyCode == Keys.End)) e.SuppressKeyPress = true;
            jisuanshu.SelectionLength = 0;
        }

        private int _lastCengIndex = -1;

        private void cengshu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AppSession.IsGuest && (cengshu.Text == "2层" || cengshu.Text == "3层"))
            {
                System.Windows.Forms.MessageBox.Show("游客不能计算2层、3层地下室，请重新启动并登录后使用。");
                cengshu.SelectedIndex = _lastCengIndex;   // 回退到上一次有效选择
                return;
            }
            _lastCengIndex = cengshu.SelectedIndex;

            string cs = cengshu.Text;
            switch (cs)
            {
                case "1层":
                    jisuanshu.Width = 709;//长，
                    jisuanshu.Height = 565;//宽。
                    jisuanshu.Location = new Point(4, 334);//坐标，

                    break;
                case "2层":
                    jisuanshu.Width = 709;//长，
                    jisuanshu.Height = 438;//宽。
                    jisuanshu.Location = new Point(7, 461);//坐标，

                    break;
                case "3层":
                    jisuanshu.Width = 709;//长，
                    jisuanshu.Height = 311;//宽。
                    jisuanshu.Location = new Point(7, 588);//坐标，

                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("此功能暂未开放，敬请期待！");              //点击显示示意图时，弹窗提示，





        }

        private void jisuan_Click(object sender, EventArgs e)
        {





            string dj = kangfudengji.Text;                             //抗浮设计等级，
            string cs = cengshu.Text;                           //地下室层数，
            switch (cs)
            {
                case "1层":
                case "2层":
                case "3层":
                    break;
                default:
                    bizhi.Visible = false;
                    jielun.Visible = false;
                    System.Windows.Forms.MessageBox.Show("请选择地下室层数！");           //判断地下室层数是否为空，
                    return;
            }
            if (gongchengmingcheng.Text.Length == 0 || quyu.Text.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("工程名称、验算区域不能留空，请输入参数！");        //非数值必填项校验
                return;
            }

            bizhi.Visible = true;                                           //比值取消隐藏状态
            jielun.Visible = true;                                           //结论取消隐藏状态

            var errors = new List<string>();
            float Get(TextBox box, string name)
            {
                if (float.TryParse(box.Text, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float v))
                {
                    return v;
                }
                errors.Add("「" + name + "」为空或数值无效");
                return 0;
            }

            double sw = Get(shuiwei, "抗浮设防水位标高");                       //抗浮水位标高
            double jcd = Get(dibandibiaogao, "基础底板底标高");                  //基础底板底标高
            float trz = Get(tongrongzhong, "混凝土容重");                       //混凝土容重
            float ydbg = Get(yidingbanbiaogao, "-1层顶板顶标高");                 //-1层顶板顶标高
            float yftrz = Get(fuyidingbanfuturongzhong, "-1层顶板覆土容重");      //-1层顶板覆土容重
            float dftrz = Get(dibanfuturongzhong, "底板覆土容重");               //底板覆土容重
            float ydbh = Get(fuyidingbanhou, "-1层顶板厚");                      //-1层顶板厚
            float yfth = Get(fuyidingbanfutuhou, "-1层顶板覆土厚");               //-1层顶板覆土厚
            float dfth = Get(dibanfutuhou, "底板覆土厚");                        //底板覆土厚

            float yxzlk = Get(fuyixzhuliangkuan, "-1层x向主梁宽");
            float yyzlk = Get(fuyiyzhuliangkuan, "-1层y向主梁宽");
            float yxzlg = Get(fuyixzhulianggao, "-1层x向主梁高");
            float yyzlg = Get(fuyiyzhulianggao, "-1层y向主梁高");
            float yxzls = Get(fuyixzhuliangshu, "-1层x向主梁数量");
            float yyzls = Get(fuyiyzhuliangshu, "-1层y向主梁数量");
            float yxclk = Get(fuyixciliangkuan, "-1层x向次梁宽");
            float yyclk = Get(fuyiyciliangkuan, "-1层y向次梁宽");
            float yxclg = Get(fuyixcilianggao, "-1层x向次梁高");
            float yyclg = Get(fuyiycilianggao, "-1层y向次梁高");
            float yxcls = Get(fuyixciliangshu, "-1层x向次梁数量");
            float yycls = Get(fuyiyciliangshu, "-1层y向次梁数量");
            float yzc = Get(fuyizhuchang, "-1层柱长度");
            float yzk = Get(fuyizhukuan, "-1层柱宽度");
            float yzs = Get(fuyizhushu, "-1层柱数量");

            float jch = Get(jichuhou, "基础厚度(含底板厚)");
            float jcc = Get(jichuchang, "基础长度");
            float jck = Get(jichukuan, "基础宽度");
            float jcs = Get(jichushuliang, "基础数量");
            float jcbh = Get(dibanhoudu, "基础底板厚度");
            float qyx = Get(quyux, "验算区域x向长度");
            float qyy = Get(quyuy, "验算区域y向长度");

            float edbg = 0, sdbg = 0;
            float eftrz = 0, sftrz = 0;
            float edbh = 0, sdbh = 0;
            float efth = 0, sfth = 0;
            float exzlk = 0, eyzlk = 0, exzlg = 0, eyzlg = 0, exzls = 0, eyzls = 0;
            float sxzlk = 0, syzlk = 0, sxzlg = 0, syzlg = 0, sxzls = 0, syzls = 0;
            float exclk = 0, eyclk = 0, exclg = 0, eyclg = 0, excls = 0, eycls = 0;
            float sxclk = 0, syclk = 0, sxclg = 0, syclg = 0, sxcls = 0, sycls = 0;
            float ezc = 0, ezk = 0, ezs = 0;
            float szc = 0, szk = 0, szs = 0;

            if (cs == "2层" || cs == "3层")
            {
                edbg = Get(erdingbanbiaogao, "-2层顶板顶标高");
                eftrz = Get(fuerdingbanfuturongzhong, "-2层顶板覆土容重");
                edbh = Get(fuerdingbanhou, "-2层顶板厚");
                efth = Get(fuerdingbanfutuhou, "-2层顶板覆土厚");
                exzlk = Get(fuerxzhuliangkuan, "-2层x向主梁宽");
                eyzlk = Get(fueryzhuliangkuan, "-2层y向主梁宽");
                exzlg = Get(fuerxzhulianggao, "-2层x向主梁高");
                eyzlg = Get(fueryzhulianggao, "-2层y向主梁高");
                exzls = Get(fuerxzhuliangshu, "-2层x向主梁数量");
                eyzls = Get(fueryzhuliangshu, "-2层y向主梁数量");
                exclk = Get(fuerxciliangkuan, "-2层x向次梁宽");
                eyclk = Get(fueryciliangkuan, "-2层y向次梁宽");
                exclg = Get(fuerxcilianggao, "-2层x向次梁高");
                eyclg = Get(fuerycilianggao, "-2层y向次梁高");
                excls = Get(fuerxciliangshu, "-2层x向次梁数量");
                eycls = Get(fueryciliangshu, "-2层y向次梁数量");
                ezc = Get(fuerzhuchang, "-2层柱长度");
                ezk = Get(fuerzhukuan, "-2层柱宽度");
                ezs = Get(fuerzhushu, "-2层柱数量");
            }
            if (cs == "3层")
            {
                sdbg = Get(sandingbanbiaogao, "-3层顶板顶标高");
                sftrz = Get(fusandingbanfuturongzhong, "-3层顶板覆土容重");
                sdbh = Get(fusandingbanhou, "-3层顶板厚");
                sfth = Get(fusandingbanfutuhou, "-3层顶板覆土厚");
                sxzlk = Get(fusanxzhuliangkuan, "-3层x向主梁宽");
                syzlk = Get(fusanyzhuliangkuan, "-3层y向主梁宽");
                sxzlg = Get(fusanxzhulianggao, "-3层x向主梁高");
                syzlg = Get(fusanyzhulianggao, "-3层y向主梁高");
                sxzls = Get(fusanxzhuliangshu, "-3层x向主梁数量");
                syzls = Get(fusanyzhuliangshu, "-3层y向主梁数量");
                sxclk = Get(fusanxciliangkuan, "-3层x向次梁宽");
                syclk = Get(fusanyciliangkuan, "-3层y向次梁宽");
                sxclg = Get(fusanxcilianggao, "-3层x向次梁高");
                syclg = Get(fusanycilianggao, "-3层y向次梁高");
                sxcls = Get(fusanxciliangshu, "-3层x向次梁数量");
                sycls = Get(fusanyciliangshu, "-3层y向次梁数量");
                szc = Get(fusanzhuchang, "-3层柱长度");
                szk = Get(fusanzhukuan, "-3层柱宽度");
                szs = Get(fusanzhushu, "-3层柱数量");
            }

            if (errors.Count > 0)
            {
                bizhi.Visible = false;
                jielun.Visible = false;
                System.Windows.Forms.MessageBox.Show("以下参数为空或数值无效，请检查输入：\r\n" + string.Join("\r\n", errors));
                return;
            }
            if (qyx <= 0 || qyy <= 0)
            {
                bizhi.Visible = false;
                jielun.Visible = false;
                System.Windows.Forms.MessageBox.Show("验算区域x向、y向长度必须大于0！");
                return;
            }

            var cans = new KangfuCans
            {
                dj = dj, cs = cs,
                sw = sw, jcd = jcd, trz = trz, ydbg = ydbg, yftrz = yftrz, dftrz = dftrz,
                ydbh = ydbh, yfth = yfth, dfth = dfth,
                yxzlk = yxzlk, yyzlk = yyzlk, yxzlg = yxzlg, yyzlg = yyzlg, yxzls = yxzls, yyzls = yyzls,
                yxclk = yxclk, yyclk = yyclk, yxclg = yxclg, yyclg = yyclg, yxcls = yxcls, yycls = yycls,
                yzc = yzc, yzk = yzk, yzs = yzs,
                jch = jch, jcc = jcc, jck = jck, jcs = jcs, jcbh = jcbh,
                qyx = qyx, qyy = qyy,
                edbg = edbg, sdbg = sdbg, eftrz = eftrz, sftrz = sftrz,
                edbh = edbh, sdbh = sdbh, efth = efth, sfth = sfth,
                exzlk = exzlk, eyzlk = eyzlk, exzlg = exzlg, eyzlg = eyzlg, exzls = exzls, eyzls = eyzls,
                sxzlk = sxzlk, syzlk = syzlk, sxzlg = sxzlg, syzlg = syzlg, sxzls = sxzls, syzls = syzls,
                exclk = exclk, eyclk = eyclk, exclg = exclg, eyclg = eyclg, excls = excls, eycls = eycls,
                sxclk = sxclk, syclk = syclk, sxclg = sxclg, syclg = syclg, sxcls = sxcls, sycls = sycls,
                ezc = ezc, ezk = ezk, ezs = ezs,
                szc = szc, szk = szk, szs = szs,
                swS = shuiwei.Text, jcdS = dibandibiaogao.Text, ydbgS = yidingbanbiaogao.Text,
                yfthS = fuyidingbanfutuhou.Text, qyxS = quyux.Text, qyyS = quyuy.Text,
                quyuS = quyu.Text, mc = gongchengmingcheng.Text,
            };
            KangfuResult r = KangfuJisuan.Jisuan(cans);
            if (!r.Youxiao)
            {
                bizhi.Visible = false;
                jielun.Visible = false;
                jisuanshu.Text = "";
                System.Windows.Forms.MessageBox.Show("请选择抗浮设计等级！");        //判断抗浮等级是否为空
                return;
            }
            bizhi.Text = r.Bizhi;
            jielun.Text = r.Jielun;
            jisuanshu.Text = r.JisuanShu;
        }

        private void tuichu_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void shengchengjisuanshu_Click(object sender, EventArgs e)
        {
            if (AppSession.IsGuest)
            {
                System.Windows.Forms.MessageBox.Show("游客不能导出计算书，请重新启动并登录后使用。");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "选择保存文件路径";                                      //对话框标题
            saveFileDialog.Filter = "txt文件（*.txt）|*.txt";  //设置过滤器
            saveFileDialog.FileName = "抗浮计算书.txt";                             //设置文件默认名称
            saveFileDialog.FilterIndex = 0;                                         //默认过滤器中类型
            saveFileDialog.RestoreDirectory = true;                                 //保存对话框是否记忆上次打开的目录
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;             //打开对话框,对话框结果不为OK则返回
            string path = saveFileDialog.FileName;
            try
            {
                File.WriteAllText(path, jisuanshu.Text);
                System.Windows.Forms.MessageBox.Show("保存成功！");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("保存失败：" + ex.Message);
            }

        }


        private void qingchu_Click(object sender, EventArgs e)         //清楚已输入的数值
        {
            bizhi.Text = jielun.Text = shuiwei.Text = dibandibiaogao.Text = tongrongzhong.Text = yidingbanbiaogao.Text = erdingbanbiaogao.Text = sandingbanbiaogao.Text = fuyidingbanfuturongzhong.Text = fuerdingbanfuturongzhong.Text =
                fusandingbanfuturongzhong.Text = fuyidingbanhou.Text = fuerdingbanhou.Text = fusandingbanhou.Text = fuyidingbanfutuhou.Text = fuerdingbanfutuhou.Text = fusandingbanfutuhou.Text = fuyixzhuliangkuan.Text =
                fuerxzhuliangkuan.Text = fusanxzhuliangkuan.Text = fuyiyzhuliangkuan.Text = fueryzhuliangkuan.Text = fusanyzhuliangkuan.Text = fuyixzhulianggao.Text = fuerxzhulianggao.Text = fusanxzhulianggao.Text = fuyiyzhulianggao.Text =
                fueryzhulianggao.Text = fusanyzhulianggao.Text = fuyixzhuliangshu.Text = fuerxzhuliangshu.Text = fusanxzhuliangshu.Text = fuyiyzhuliangshu.Text = fueryzhuliangshu.Text = fusanyzhuliangshu.Text =
                fuyixciliangkuan.Text = fuerxciliangkuan.Text = fusanxciliangkuan.Text = fuyiyciliangkuan.Text = fueryciliangkuan.Text = fusanyciliangkuan.Text = fuyixcilianggao.Text = fuerxcilianggao.Text = fusanxcilianggao.Text =
                fuyiycilianggao.Text = fuerycilianggao.Text = fusanycilianggao.Text = fuyixciliangshu.Text = fuerxciliangshu.Text = fusanxciliangshu.Text = fuyiyciliangshu.Text = fueryciliangshu.Text = fusanyciliangshu.Text =
                fuyizhuchang.Text = fuerzhuchang.Text = fusanzhuchang.Text = fuyizhukuan.Text = fuerzhukuan.Text = fusanzhukuan.Text = fuyizhushu.Text = fuerzhushu.Text = fusanzhushu.Text = jichuhou.Text = jichuchang.Text = jichukuan.Text =
                jichushuliang.Text = dibanhoudu.Text = quyux.Text = quyuy.Text = dibanfuturongzhong.Text = quyu.Text = dibanfutuhou.Text = gongchengmingcheng.Text = "";
            cengshu.Text = kangfudengji.Text = "";
        }

        #region 数值输入限制（共享）
        private static void RestrictNumericInput(KeyPressEventArgs e, bool allowMinus, bool allowDecimal)
        {
            bool ok = (e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '\b'
                || (allowDecimal && e.KeyChar == '.')
                || (allowMinus && e.KeyChar == '-');
            if (!ok)
            {
                System.Windows.Forms.MessageBox.Show(allowDecimal ? "请输入正确的数值" : "请输入正确的整数");
                e.Handled = true;
            }
        }

        private void SignedDecimal_KeyPress(object sender, KeyPressEventArgs e) => RestrictNumericInput(e, allowMinus: true, allowDecimal: true);

        private void Decimal_KeyPress(object sender, KeyPressEventArgs e) => RestrictNumericInput(e, allowMinus: false, allowDecimal: true);

        private void Integer_KeyPress(object sender, KeyPressEventArgs e) => RestrictNumericInput(e, allowMinus: false, allowDecimal: false);
        #endregion

    }
}
