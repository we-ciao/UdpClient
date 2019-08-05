using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace DampingForwardUDP
{
    public partial class Form1 : Form
    {
        //UDP服务器
        AsyncUDPServer udpReceive;
        AsyncUDPServer udpSend;
        //定义ip
        private const string listenIp = "10.14.123.101";
        private const string remoteIp = "10.14.123.120";
        //private const string listenIp = "192.168.1.99";
        //private const string remoteIp = "10.14.123.120";
        // 定义端口
        private const int listenPort = 61000;
        private const int remotePort = 1231;
        // 定义节点
        private IPEndPoint ipEndPoint = null;
        private IPEndPoint remoteEP = null;

        //算法参数
        //原始数据取前fcount个
        int fcount = 5;
        //拉索固有频率有效极值点个数（默认为 6 个）
        int ccount = 5;
        //10.5Hz以上的的频率，不计入计算范围
        double fout = 10.5;
        //计算z组基频使用的数据个数n= zcount*fcount
        int zcount = 5;

        public Form1()
        {
            InitializeComponent();
            lstIP.HorizontalScrollbar = true;

            // 本机节点
            ipEndPoint = new IPEndPoint(IPAddress.Parse(listenIp), listenPort);
            // 远程节点
            remoteEP = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);

            //UDP服务器
            udpReceive = new AsyncUDPServer(ipEndPoint);
            udpReceive.DataReceived += ReceiveCallback;
            udpReceive.OtherException += ErrorCallback;

            udpSend = new AsyncUDPServer(0);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            delelteOldTxt();
            getSetting();

            udpReceive.Start();
            Invoke(new MethodInvoker(delegate ()
            {
                //使Inovke或者BenginInvoke，提交给主线程处理。
                txtMsg.AppendText("接收" + listenIp + ":" + listenPort + "\n");
            }));

            //添加未接收的传感器
            for (int i = 0; i <= 151; i++)
            {
                lstIndex.Items.Add(i);
            }

        }

        // 接收回调函数
        private void ErrorCallback(Object sender, EventArgs e)
        {
            var err = e as AsyncUDPEventArgs;
            MessageBox.Show(err._msg);
        }
        // 接收回调函数
        private void ReceiveCallback(Object sender, EventArgs e)
        {
            var receviestate = (e as AsyncUDPEventArgs)._state;
            Byte[] receiveBytes = receviestate.buffer;
            string receiveString = System.Text.Encoding.Default.GetString(receiveBytes);
            var ip = receviestate.remote.Address + ":" + receviestate.remote.Port;
            string msg = string.Format("{2}[来自{0}]: {1}", ip, receiveString, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            Invoke(new MethodInvoker(delegate ()
            {
                txtMsg.AppendText(msg + "\n");
            }));

            var data = receiveString.Split(',');
            switch (data.Count())
            {
                case 7:
                    monitor(data);
                    break;
                case 36:
                    frequencyspectrum(data);
                    break;

                default:
                    break;
            }
            savetxt(receiveString, ip);

            //删除未连接设备
            for (int i = 0; i < lstIndex.Items.Count; i++)
            {
                if (lstIndex.Items[i].ToString() == data[1])
                {
                    Invoke(new MethodInvoker(delegate ()
                    {

                        lstIndex.Items.Remove(lstIndex.Items[i]);
                    }));
                    break;
                }
            }
            //已连接的设备
            Invoke(new MethodInvoker(delegate ()
            {
                int i = 0, count = lstAll.Items.Count;
                for (i = 0; i < count; i++)
                {
                    if (lstAll.Items[i].ToString() == data[1].ToString())
                    {
                        break;
                    }
                }
                if (i == count)
                    lstAll.Items.Add(data[1]);
            }));
        }

        // 发送函数
        private void SendMsg(string receiveString, int sid)
        {
            Byte[] sendBytes = System.Text.Encoding.Default.GetBytes(receiveString);
            udpSend.Send(sendBytes, remoteEP);
            // lstIP.Invoke(setlistBoxCallback, "转发 [" + sid + "] 基频: 监控数据UDP报文");
        }

        #region 算法部分
        //基频
        List<double> f = new List<double>();
        private Dictionary<int, List<ValueCount>> dataDic = new Dictionary<int, List<ValueCount>>();

        //阻尼器振动的频谱UDP报文
        public void frequencyspectrum(String[] data)
        {
            int sid = Convert.ToInt32(data[1]);
            if (!dataDic.ContainsKey(sid))
                dataDic[sid] = new List<ValueCount>();


            List<ValueCount> VCdata = new List<ValueCount>();
            for (int i = 3; i < data.Length - 1; i += 2)
            {
                //转换频率
                var value = (Convert.ToDouble(data[i]) * 25) / 2048.0;
                //10.5Hz以上的的频率，不计入计算范围
                if (value < fout)
                {
                    ValueCount valueCount = new ValueCount(value, Convert.ToInt32(data[i + 1]));
                    VCdata.Add(valueCount);
                }
            }
            var cc = VCdata.OrderByDescending(x => x.count).Take(fcount);

            lock (dataDic)//锁
            {
                dataDic[sid].AddRange(cc);

                if (dataDic[sid].Count >= zcount * fcount)
                {
                    var res = algorithm(dataDic[sid], sid);
                    dataDic[sid].RemoveRange(0, fcount);
                    SendFrequencySpectrum(res, sid);
                }
            }
        }


        public double algorithm(List<ValueCount> freqList, int sid)
        {
            //频谱图中对识别拉索固有频率有效的极值点个数（默认为 6 个）  
            List<double> amplitude = new List<double>();

            //是否有配置f
            int fid = sid < f.Count ? sid - 1 : 0;
            //筛选过程
            var statisList = StatisticsCount(freqList, f[fid])
                .OrderByDescending(x => x.Count)
                .Take(ccount);

            //分组后求平均值
            var freqOrder = new List<ValueCount>();
            foreach (var item in statisList)
            {
                freqOrder.Add(new ValueCount(item.Average(x => x.value), item.Average(x => x.count)));
            }
            freqOrder.Sort();

            List<double> extremum = new List<double>();
            while (freqOrder.Count() > 1)
            {
                extremum.AddRange(ValueDifference(freqOrder));
                //删除y最小的
                double min = freqOrder.FirstOrDefault().count;
                int index = 0;
                for (int i = 0; i < freqOrder.Count; i++)
                {
                    if (freqOrder[i].count < min)
                        index = i;
                }
                freqOrder.RemoveAt(index);
            }

            List<ValueCount> mulroundmum = new List<ValueCount>();
            extremum.ForEach(x =>
            //乘以 5 之后取整
            mulroundmum.Add(new ValueCount(x, (int)Math.Round(x * 5, 0, MidpointRounding.AwayFromZero))));

            var ss = mulroundmum.GroupBy(x => x.count).OrderByDescending(x => x.Count()).FirstOrDefault().ToList();
            var result = ss.Sum(x => x.value) / ss.Count;

            return result;
        }

        /// <summary>
        /// 统计An-As<0.1f个数
        /// </summary>
        /// <param name="list">统计集合</param>
        /// <param name="f">基频</param>
        public static List<List<ValueCount>> StatisticsCount(List<ValueCount> list, double f)
        {
            double fmax = 0.1 * f;
            List<List<ValueCount>> res = new List<List<ValueCount>>();

            for (int i = 0; i < list.Count; i++)
            {
                bool flag = true;

                foreach (var item in res)
                {
                    double c = Math.Abs(item.FirstOrDefault().value - list[i].value);

                    if ((c <= fmax))
                    {
                        flag = false;
                        item.Add(list[i]);
                        continue;
                    }
                }
                if (flag)
                    res.Add(new List<ValueCount>() { list[i] });
            }

            return res;
        }


        /// <summary>
        /// 计算每两个数的差
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<double> ValueDifference(List<ValueCount> e)
        {
            List<double> res = new List<double>();
            for (int i = 0; i < e.Count - 1; i++)
            {
                res.Add(e[i + 1].value - e[i].value);
            }
            return res;
        }
        #endregion


        /// <summary>
        /// 振动监控数据UDP报文
        /// </summary>
        /// <param name="data"></param>
        public void monitor(String[] data)
        {
            int sid = Convert.ToInt32(data[1]);
            double Xmin = Convert.ToDouble(data[2]);
            double Xmax = Convert.ToDouble(data[3]);
            double Ymin = Convert.ToDouble(data[4]);
            double Ymax = Convert.ToDouble(data[5]);

            SendMsg(Xmin.ToString(), sid);
        }

        // 转发计算阻尼器振动的频谱UDP报文
        private void SendFrequencySpectrum(double receive, int sid)
        {
            Byte[] sendBytes = new Byte[20];
            sendBytes[0] = 0x02;                                             //--通用报文
            sendBytes[1] = 0x00;                                             //--预留
            sendBytes[2] = 0x01;                                             //--方向，上行
            sendBytes[3] = 0x00;                                             //--默认，通讯计算机编号
            sendBytes[4] = 0x01;                                             //--命令码
            sendBytes[5] = 0x0D;                                             //--报文长度，13字节
            sendBytes[6] = 0x00;
            sendBytes[7] = Convert.ToByte(DateTime.Now.Year - 1900);         //--年，2019减去1900，为119
            sendBytes[8] = Convert.ToByte(DateTime.Now.Month);               //--3月
            sendBytes[9] = Convert.ToByte(DateTime.Now.Day);                 //--25日
            sendBytes[10] = Convert.ToByte(DateTime.Now.Hour);               //--10时
            sendBytes[11] = Convert.ToByte(DateTime.Now.Minute);             //--07分
            var temp = BitConverter.GetBytes(DateTime.Now.Millisecond + DateTime.Now.Second * 1000);
            sendBytes[12] = temp[1];                                          //--毫秒数量
            sendBytes[13] = temp[0];
            temp = BitConverter.GetBytes(sid);
            sendBytes[14] = temp[1];                                          //--对象码
            sendBytes[15] = temp[0];
            temp = BitConverter.GetBytes((float)receive);
            sendBytes[16] = temp[3];                                          //--数据值，float浮点型
            sendBytes[17] = temp[2];
            sendBytes[18] = temp[1];
            sendBytes[19] = temp[0];

            udpSend.Send(sendBytes, remoteEP);

            Invoke(new MethodInvoker(delegate ()
            {
                lstIP.Items.Add("转发 [" + sid + "] 基频: " + receive);
            }));
        }

        public static object filelocker = new object();
        /// <summary>
        /// 保存原始数据
        /// </summary>
        /// <param name="msg"></param>
        private void savetxt(string msg, string ip)
        {
            string strDestination = Application.StartupPath + "\\data\\" + DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";
            string strPath = Path.GetDirectoryName(strDestination);
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }

            lock (filelocker)//锁
            {
                StreamWriter sw = new StreamWriter(strDestination, true);
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "[" + ip + "]" + msg);
                sw.Close();//写入
            }
        }

        /// <summary>
        /// 读取基频
        /// </summary>
        private void getSetting()
        {
            try
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\frequency.txt", false);
                string temp = null;
                while ((temp = sr.ReadLine()) != null)
                {
                    f.Add(Convert.ToDouble(temp));
                }
                sr.Close();
            }
            catch (Exception)
            {
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\frequency.txt", false);
                sw.WriteLine("1.7");
                sw.Close();//写入
            }
        }

        /// <summary>
        /// 删除半年前的数据
        /// </summary>
        private void delelteOldTxt()
        {
            //文件夹路径
            string strFolderPath = Application.StartupPath + "\\data\\";
            if (!Directory.Exists(strFolderPath))
            {
                Directory.CreateDirectory(strFolderPath);
            }
            DirectoryInfo dyInfo = new DirectoryInfo(strFolderPath);
            //获取文件夹下所有的文件
            foreach (FileInfo feInfo in dyInfo.GetFiles())
            {
                //判断文件日期是否小于今天，是则删除
                if (feInfo.CreationTime < DateTime.Today.AddMonths(-6))
                    feInfo.Delete();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            udpReceive.Dispose();
            Application.Exit();
        }

        private void TxtMsg_TextChanged(object sender, EventArgs e)
        {
            //txtMsg.SelectionStart = txtMsg.TextLength;
            //txtMsg.ScrollToCaret();

           lstIP.TopIndex = lstIP.Items.Count - 1;
        }

    }
}
