using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AntRuntime.Message
{
    /// <summary>
    /// 消息文件存储格式
    /// FileHead(64)+Block pointer(8)*24+[Message block]
    /// FileHead(文件头):datetime(8)+databasename(32)+other(24)
    /// Block pointer(Block 指针)(8)
    /// [Message block](消息数据块集合)
    /// </summary>
    public class MessageFileSerise
    {

        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...

        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// 
        /// </summary>
        public List<MessageBlockBuffer> Result { get; set; }
        #endregion ...Properties...

        #region ... Methods    ...

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public MessageFileSerise Load(System.IO.Stream stream)
        {
            return Load(stream, 0, 24);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lid"></param>
        /// <returns></returns>
        public DateTime RestoreTimeFromId(long lid)
        {
            var vid = lid / 10;
            return DateTime.FromBinary(vid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public MessageFileSerise Load(System.IO.Stream stream, long id)
        {
            Result = new List<MessageBlockBuffer>();
            DateTime dt = RestoreTimeFromId(id);
            using (var br = new System.IO.BinaryReader(stream, Encoding.UTF8, true))
            {
                stream.Position = 64 + dt.Hour * 8;
                long offset = br.ReadInt64();
                stream.Position = offset;

                MessageBlockBuffer mbb = new MessageBlockBuffer() { Hour = dt.Hour };
                mbb.Load(stream);
                Result.Add(mbb);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mHours"></param>
        /// <returns></returns>
        public MessageFileSerise Load(System.IO.Stream stream, List<int> mHours)
        {
            Result = new List<MessageBlockBuffer>();
            List<long> ltmp = new List<long>();

            if (stream.Length == 0) return this;

            using (var br = new System.IO.BinaryReader(stream, Encoding.UTF8, true))
            {
                foreach (var vv in mHours)
                {
                    stream.Position = 64 + vv * 8;
                    ltmp.Add(br.ReadInt64());
                }
            }
            int i = 0;
            foreach (var vv in ltmp)
            {
                stream.Position = vv;
                MessageBlockBuffer mbb = new MessageBlockBuffer() { Hour=-1};
                if (vv >= (64 + 24 * 8))
                {
                    mbb.Hour = mHours[i];
                    mbb.Load(stream);
                    mbb.FilePosition = vv;
                }
                Result.Add(mbb);
                i++;
            }
            return this;
        }


        /// <summary>
        /// 
        /// </summary>
        public MessageFileSerise Load(System.IO.Stream stream,int from,int to)
        {
            Result = new List<MessageBlockBuffer>();
            Dictionary<int,long> ltmp = new Dictionary<int, long>();
            //确保读取一个
            if (from == to) to++;

            using (var br = new System.IO.BinaryReader(stream, Encoding.UTF8, true))
            {
                for (int i = from; i < to; i++)
                {
                    stream.Position = 64 + i * 8;
                    ltmp.Add(i,br.ReadInt64());
                }
            }

            foreach(var vv in ltmp)
            {
                stream.Position = vv.Value;
                MessageBlockBuffer mbb = new MessageBlockBuffer() { Hour = vv.Key };
                mbb.Load(stream);
                Result.Add(mbb);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        public static void Save(MessageBlockBuffer block, System.IO.Stream stream)
        {
            //文件指针便宜
            int head = 64 + 8 * 24;
            var vbb = stream.Length < head ? head : stream.Length;
            using (var br = new System.IO.BinaryWriter(stream, Encoding.UTF8, true))
            {
                stream.Position = block.Hour * 8 + 64;
                br.Write(vbb);
            }
            stream.Position = vbb;
            block.Save(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <param name="stream"></param>
        public static void UpdateDirtyToDisk(MessageBlockBuffer block,System.IO.Stream stream)
        {
            using (var br = new System.IO.BinaryReader(stream, Encoding.UTF8, true))
            {
                stream.Position = block.Hour * 8 + 64;
                stream.Position = br.ReadInt64();
                block.UpdateDirtyToDisk(stream);
            }
        }

        #endregion ...Methods...

        #region ... Interfaces ...

        #endregion ...Interfaces...
    }

    /// <summary>
    /// 消息块存储格式
    /// blockheader(16)+alarmMessageBody+messageBody
    /// blockheader(块文件头):Datasize(8)(数据块大小)+messageBodyOffset(4)(一般报警消息偏移量)
    /// alarmMessageBody(报警消息数据)
    /// messageBody(一般消息数据)
    /// </summary>
    public class MessageBlockBuffer:IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public int Hour { get; set; }

        /// <summary>
        /// 数据在文件中偏移
        /// </summary>
        public long FilePosition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public AlarmMessageAreaBuffer AlarmArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CommonMessageAreaBuffer CommonArea { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            AlarmArea?.Dispose();
            CommonArea?.Dispose();
            AlarmArea = null;
            CommonArea = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> GetMessages(DateTime starttime,DateTime endtime)
        {
            SortedDictionary<long, Cdy.Ant.Tag.Message> ltmp = new SortedDictionary<long, Cdy.Ant.Tag.Message>();

            if (AlarmArea != null && AlarmArea.AlarmMessage != null)
            {
                foreach (var vv in AlarmArea.AlarmMessage.Values)
                {
                    if(vv.CreateTime>=starttime && vv.CreateTime<endtime)
                    ltmp.Add(vv.Id, vv);
                }
            }

            if (CommonArea != null && CommonArea.Message != null)
            {
                foreach (var vv in CommonArea.Message.Values)
                {
                    if (vv.CreateTime >= starttime && vv.CreateTime < endtime)
                        ltmp.Add(vv.Id, vv);
                }
            }

            return ltmp.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cdy.Ant.Tag.Message> GetMessages()
        {
            SortedDictionary<long, Cdy.Ant.Tag.Message> ltmp = new SortedDictionary<long, Cdy.Ant.Tag.Message>();

            if (AlarmArea != null && AlarmArea.AlarmMessage != null)
            {
                foreach (var vv in AlarmArea.AlarmMessage.Values)
                {
                    ltmp.Add(vv.Id, vv);

                }
            }

            if (CommonArea != null && CommonArea.Message != null)
            {
                foreach (var vv in CommonArea.Message.Values)
                {
                    ltmp.Add(vv.Id, vv);

                }
            }

            return ltmp.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="seek"></param>
        public unsafe void Load(System.IO.Stream stream)
        {
            using(var br = new System.IO.BinaryReader(stream,Encoding.UTF8,true))
            {
                var vsize = br.ReadInt32();
                var offset = br.ReadInt32();
                IntPtr ptr = Marshal.AllocHGlobal((int)vsize);
                br.BaseStream.Read(new Span<byte>((void*)ptr, (int)vsize));

                AlarmArea = new AlarmMessageAreaBuffer();

                AlarmArea.Load(ptr);
                CommonArea = new CommonMessageAreaBuffer();

                CommonArea.Load(ptr, (int)offset);

                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public unsafe void Save(System.IO.Stream stream)
        {
            using (var br = new System.IO.BinaryWriter(stream, Encoding.UTF8, true))
            {
                AlarmArea.Save();
                CommonArea.Save();

                var vsize = AlarmArea.DataSize + CommonArea.DataSize;
                var offset = AlarmArea.DataSize;
                br.Write(vsize);
                br.Write(offset);

                if (AlarmArea.DataPointer != IntPtr.Zero)
                {
                    using (System.IO.UnmanagedMemoryStream ss = new System.IO.UnmanagedMemoryStream((byte*)AlarmArea.DataPointer, AlarmArea.DataSize))
                    {
                        ss.CopyTo(stream);
                    }
                }

                if(CommonArea.DataPointer!=IntPtr.Zero)
                {
                    using (System.IO.UnmanagedMemoryStream ss = new System.IO.UnmanagedMemoryStream((byte*)CommonArea.DataPointer, CommonArea.DataSize))
                    {
                        ss.CopyTo(stream);
                    }
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        public void UpdateDirtyToDisk(System.IO.Stream stream)
        {
            //Block头部两个Int整形数据：数据大小+一般区域地址偏移
            stream.Position += 4;

            int compos = 0;
            //读取一般消息，存储区地址
            using(var br = new System.IO.BinaryReader(stream,Encoding.UTF8,true))
            {
                compos = br.ReadInt32();
            }

            var pos = stream.Position;
            this.AlarmArea?.UpdateChanged(stream);

            stream.Position = pos+compos;
            this.CommonArea?.UpdateChanged(stream);
        }


    }

    /// <summary>
    /// 报警消息存储区域
    /// area header(4+4+4)+message data
    /// area header(12): count(消息个数)+restoreDataPoint(4)+ackDataPoint(4)
    /// message data(消息数据):[messageid(8)]+AlarmMessageBody(zip)+[RestoreTime(8)+RestoreValue(64)]+[AckTime(8)+AckMessage(64)+AckUser(32)]
    /// AlarmMessageBody(报警消息体,压缩内容):[Server(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)+AlarmLevel(1)+AlarmValue(64)+AlarmCondition(64)+LinkTag(64)]
    /// </summary>
    public class AlarmMessageAreaBuffer:IDisposable
    {

        private Dictionary<long, Cdy.Ant.Tag.AlarmMessage> mAlarmMessages = new Dictionary<long, Cdy.Ant.Tag.AlarmMessage>();

        private IntPtr mDataPointer;

        /// <summary>
        /// 
        /// </summary>
        public IntPtr DataPointer
        {
            get
            {
                return mDataPointer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int DataSize { get; set; }

        /// <summary>
        /// 恢复区地址偏移
        /// </summary>
        public int RestoreOffset { get; set; }
        /// <summary>
        /// 确认区地址偏移
        /// </summary>
        public int AckOffset { get; set; }

        /// <summary>
        /// 删除区地址偏移
        /// </summary>
        public int DeleteOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAckDirty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDeleteDirty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRestoreDirty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<long, Cdy.Ant.Tag.AlarmMessage> AlarmMessage
        {
            get
            {
                return mAlarmMessages;
            }
            set
            {
                mAlarmMessages = value;
            }
        }

       

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (mDataPointer != IntPtr.Zero)
                Marshal.FreeHGlobal(mDataPointer);
            mAlarmMessages.Clear();


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="seek"></param>
        public unsafe void Load(IntPtr pointer)
        {
            mDataPointer = pointer;
            int offset = 0;
            var mcount = MemoryHelper.ReadInt32(pointer, offset);
            offset += 4;

            RestoreOffset = MemoryHelper.ReadInt32(pointer, offset);
            offset += 4;

            AckOffset = MemoryHelper.ReadInt32(pointer, offset);
            offset += 4;

            DeleteOffset = MemoryHelper.ReadInt32(pointer, offset);
            offset += 4;

            //List<long> lids = new List<long>(mcount);
            mAlarmMessages = new Dictionary<long, Cdy.Ant.Tag.AlarmMessage>();
            var mtmp = new List<Cdy.Ant.Tag.AlarmMessage>(mcount);
            //for (int i = 0; i < mcount; i++)
            //{
            //    lids.Add(MemoryHelper.ReadInt64(pointer, offset));
            //    offset += 8;
            //}

            int dsize = MemoryHelper.ReadInt32(pointer, offset);
            offset += 4;

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            {
                using (var vss = new System.IO.UnmanagedMemoryStream((byte*)pointer + offset, dsize))
                {
                    using (System.IO.Compression.BrotliStream bs = new System.IO.Compression.BrotliStream(vss, System.IO.Compression.CompressionMode.Decompress, true))
                    {
                        bs.CopyTo(ms);
                    }
                }

                ms.Position = 0;
                var tr = new System.IO.StreamReader(ms, Encoding.UTF8);

                int i = 0;
                while (!tr.EndOfStream)
                {
                    string msgbd = tr.ReadLine();
                    if (!string.IsNullOrEmpty(msgbd))
                    {
                        var almmsg = msgbd.Split(new char[] { ',' });
                        Cdy.Ant.Tag.AlarmMessage am = new Cdy.Ant.Tag.AlarmMessage();
                        //[Id+Server(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)+AlarmLevel(1)+AlarmValue(64)+AlarmCondition(64)+LinkTag(64)]
                        am.Id = long.Parse(almmsg[0]); ;
                        am.Server = almmsg[1];
                        am.SourceTag = almmsg[2];
                        am.CreateTime = DateTime.FromBinary(long.Parse(almmsg[3]));
                        am.MessageBody = almmsg[4];
                        am.AppendContent1 = almmsg[5];
                        am.AppendContent2 = almmsg[6];
                        am.AppendContent3 = almmsg[7];
                        am.AlarmLevel = (Cdy.Ant.AlarmLevel)(int.Parse(almmsg[8]));
                        am.AlarmValue = almmsg[9];
                        am.AlarmCondition = almmsg[10];
                        am.LinkTag = almmsg[11];
                        mAlarmMessages.Add(am.Id,am);
                        mtmp.Add(am);
                    }
                    i++;
                }
            }

            offset += dsize;

            offset = RestoreOffset;

            for(int i=0;i<mcount;i++)
            {
                var vtime = MemoryHelper.ReadDateTime((void*)pointer,offset);
                offset += 8;
                var sval = MemoryHelper.ReadString((void*)pointer , offset, 64);
                offset += 64;
                var vtag = mtmp[i];
                vtag.RestoreTime = vtime;
                vtag.RestoreValue = sval;
            }

            offset = AckOffset;

            for (int i = 0; i < mcount; i++)
            {
                var vtime = DateTime.FromBinary(MemoryHelper.ReadInt64((void*)pointer, offset));
                offset += 8;
                var sval = MemoryHelper.ReadString((void*)(pointer), offset,64);
                offset += 64;
                var suser = MemoryHelper.ReadString((void*)(pointer), offset, 32);
                offset += 32;

                var vtag = mtmp[i];
                vtag.AckTime = vtime;
                vtag.AckMessage = sval;
                vtag.AckUser = suser;
            }

            offset = DeleteOffset;

            for (int i = 0; i < mcount; i++)
            {
                var vtime = DateTime.FromBinary(MemoryHelper.ReadInt64((void*)pointer, offset));
                offset += 8;
                var sval = MemoryHelper.ReadString((void*)(pointer), offset, 64);
                offset += 64;
                var suser = MemoryHelper.ReadString((void*)(pointer), offset, 32);
                offset += 32;

                var vtag = mtmp[i];
                vtag.DeleteTime = vtime;
                vtag.DeleteNote = sval;
                vtag.DeleteUser = suser;
            }
            mtmp.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateChanged(System.IO.Stream stream)
        {
            var pos = stream.Position;
            if(IsAckDirty)
            {
                stream.Position += AckOffset;
                UpdateAckChanged(stream);
                IsAckDirty = false;
            }

            if(IsRestoreDirty)
            {
                stream.Position = pos + RestoreOffset;
                UpdateRestoreChange(stream);
                IsRestoreDirty = false;
            }

            if (IsDeleteDirty)
            {
                stream.Position = pos + DeleteOffset;
                UpdateDeleteChanged(stream);
                IsDeleteDirty = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private unsafe void UpdateRestoreChange(System.IO.Stream stream)
        {
            var datasize = mAlarmMessages.Values.Count * 72;
            var mDataPointer = Marshal.AllocHGlobal((int)datasize);
            new Span<byte>((void*)mDataPointer, datasize).Fill(0);
            int offset = 0;
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.RestoreTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.RestoreValue == null ? "" : vv.RestoreValue, 64);
                offset += 64;
            }

            using (System.IO.UnmanagedMemoryStream ss = new System.IO.UnmanagedMemoryStream((byte*)mDataPointer, datasize))
            {
                ss.CopyTo(stream);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private unsafe void UpdateAckChanged(System.IO.Stream stream)
        {
            var datasize = mAlarmMessages.Values.Count * 104;
            var mDataPointer = Marshal.AllocHGlobal((int)datasize);
            new Span<byte>((void*)mDataPointer, datasize).Fill(0);
           
            int offset = 0;
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.AckTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.AckMessage == null ? "" : vv.AckMessage, 64);
                offset += 64;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.AckUser == null ? "" : vv.AckUser, 32);
                offset += 32;
            }

            using (System.IO.UnmanagedMemoryStream ss = new System.IO.UnmanagedMemoryStream((byte*)mDataPointer, datasize))
            {
                ss.CopyTo(stream);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private unsafe void UpdateDeleteChanged(System.IO.Stream stream)
        {
            var datasize = mAlarmMessages.Values.Count * 104;
            var mDataPointer = Marshal.AllocHGlobal((int)datasize);
            new Span<byte>((void*)mDataPointer, datasize).Fill(0);

            int offset = 0;
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.DeleteTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteNote == null ? "" : vv.DeleteNote, 64);
                offset += 64;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteUser == null ? "" : vv.DeleteUser, 32);
                offset += 32;
            }

            using (System.IO.UnmanagedMemoryStream ss = new System.IO.UnmanagedMemoryStream((byte*)mDataPointer, datasize))
            {
                ss.CopyTo(stream);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe void Save()
        {

            System.IO.MemoryStream mCompressBuffer = new System.IO.MemoryStream();
            using (System.IO.Compression.BrotliStream bs = new System.IO.Compression.BrotliStream(mCompressBuffer, System.IO.Compression.CompressionLevel.Optimal, true))
            {
                foreach (var vv in mAlarmMessages.Values)
                {
                    //Id(8)+ Server(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)+AlarmLevel(1)+AlarmValue(64)+AlarmCondition(64)+LinkTag(64)
                    string str = vv.Id +"," +vv.Server + "," + vv.SourceTag + "," + vv.CreateTime.Ticks + "," + vv.MessageBody + "," + vv.AppendContent1 + "," + vv.AppendContent2 + "," + vv.AppendContent3 + "," + (int)vv.AlarmLevel + "," + vv.AlarmValue + "," + vv.AlarmCondition + "," + vv.LinkTag + "\r\n";
                    bs.Write(Encoding.UTF8.GetBytes(str));
                }
            }

            var mcount = mAlarmMessages.Count;

            var mcdatasize = mCompressBuffer.Position;

            long datacount = 20 + mcdatasize + mcount * (104+104+72);

            mDataPointer = Marshal.AllocHGlobal((int)datacount);
            //清空数据
            new Span<byte>((byte*)mDataPointer, (int)datacount).Fill(0);

            DataSize = (int)datacount;

            int offset = 0;

            //消息条数
            MemoryHelper.WriteInt32((void*)mDataPointer, offset, mcount);
            offset += 4;

            //Restore 数据地址
            MemoryHelper.WriteInt32((void*)mDataPointer, offset, (int)(mcdatasize + 20));
            offset += 4;

            //Ack 数据地址
            MemoryHelper.WriteInt32((void*)mDataPointer, offset, (int)(mcdatasize + 20 + mcount * 72));
            offset += 4;

            //Delete 数据地址
            MemoryHelper.WriteInt32((void*)mDataPointer, offset, (int)(mcdatasize + 20 + mcount * 72 + mcount * 104));
            offset += 4;

            //写入压缩数据包大小
            MemoryHelper.WriteInt32((void*)mDataPointer, offset, (int)(mcdatasize));
            offset += 4;

            //写入压缩数据
            Marshal.Copy(mCompressBuffer.GetBuffer(), 0, mDataPointer + offset, (int)mcdatasize);

            offset += (int)mcdatasize;
            //写入恢复值
            //offset = (int)(mcdatasize + mcount * 8 + 12+4);
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.RestoreTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.RestoreValue == null ? "" : vv.RestoreValue, 64);
                offset += 64;
            }


            //写入确认值
            //offset = (int)(mcdatasize + mcount * 8 + 12 +4 + mcount * 72 );
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.AckTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.AckMessage == null ? "" : vv.AckMessage, 64);
                offset += 64;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.AckUser == null ? "" : vv.AckUser, 32);
                offset += 32;
            }

            //写入删除信息
            //offset = (int)(mcdatasize + mcount * 8 + 12 + 4 + mcount * 72 + mcount*104);
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.DeleteTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteNote == null ? "" : vv.DeleteNote, 64);
                offset += 64;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteUser == null ? "" : vv.DeleteUser, 32);
                offset += 32;
            }

        }

    }

    /// <summary>
    /// 一般消息存储区域
    /// area header+message data(zip)
    /// area header(4):count(消息个数)
    /// message data:messageid(8)+source(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)
    /// </summary>
    public class CommonMessageAreaBuffer:IDisposable
    {

        private Dictionary<long, Cdy.Ant.Tag.Message> mAlarmMessages = new Dictionary<long, Cdy.Ant.Tag.Message>();
        /// <summary>
        /// 
        /// </summary>
        private IntPtr mDataPointer;

        /// <summary>
        /// 
        /// </summary>
        public IntPtr DataPointer
        {
            get
            {
                return mDataPointer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int DataSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDeleteDirty { get; set; }

        /// <summary>
        /// 删除区地址偏移
        /// </summary>
        public int DeleteOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<long, Cdy.Ant.Tag.Message> Message
        {
            get
            {
                return mAlarmMessages;
            }
            set
            {
                mAlarmMessages = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if(mDataPointer!=IntPtr.Zero)
            {
                Marshal.FreeHGlobal(mDataPointer);
            }
            mAlarmMessages.Clear();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="seek"></param>
        public unsafe void Load(IntPtr pointer,int offt)
        {
            mDataPointer = pointer;
            int offset = offt;
            var mcount = MemoryHelper.ReadInt32(pointer, offset);
            offset += 4;

            int dsize = MemoryHelper.ReadInt32(pointer, offset);
            offset += 4;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (var vss = new System.IO.UnmanagedMemoryStream((byte*)pointer + offset, dsize))
                {
                    using (System.IO.Compression.BrotliStream bs = new System.IO.Compression.BrotliStream(vss, System.IO.Compression.CompressionMode.Decompress, true))
                    {
                        bs.CopyTo(ms);
                    }
                }

                ms.Position = 0;
                var tr = new System.IO.StreamReader(ms,Encoding.UTF8);
                int i = 0;
                var mtmp = new List<Cdy.Ant.Tag.InfoMessage>(mcount);
                while (!tr.EndOfStream)
                {
                    string msgbd = tr.ReadLine();
                    if (!string.IsNullOrEmpty(msgbd))
                    {
                        var almmsg = msgbd.Split(new char[] { ',' });
                        Cdy.Ant.Tag.InfoMessage am = new Cdy.Ant.Tag.InfoMessage();
                        //messageid(8)+source(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)
                        am.Id = long.Parse(almmsg[0]);
                        am.Server = almmsg[1];
                        am.SourceTag = almmsg[2];
                        am.CreateTime = DateTime.FromBinary(long.Parse(almmsg[3]));
                        am.MessageBody = almmsg[4];
                        am.AppendContent1 = almmsg[5];
                        am.AppendContent2 = almmsg[6];
                        am.AppendContent3 = almmsg[7];
                        mtmp.Add(am);
                        mAlarmMessages.Add(am.Id,am);
                    }
                    i++;
                }

                offset += dsize;

                DeleteOffset = offset - offt;

                for (i = 0; i < mcount; i++)
                {
                    var vtime = DateTime.FromBinary(MemoryHelper.ReadInt64((void*)pointer, offset));
                    offset += 8;
                    var sval = MemoryHelper.ReadString((void*)(pointer), offset, 64);
                    offset += 64;
                    var suser = MemoryHelper.ReadString((void*)(pointer), offset, 32);
                    offset += 32;

                    var vtag = mtmp[i];
                    vtag.DeleteTime = vtime;
                    vtag.DeleteNote = sval;
                    vtag.DeleteUser = suser;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe void Save()
        {
            System.IO.MemoryStream mCompressBuffer = new System.IO.MemoryStream();

            using (System.IO.Compression.BrotliStream bs = new System.IO.Compression.BrotliStream(mCompressBuffer, System.IO.Compression.CompressionLevel.Fastest,true))
            {
                foreach (var vv in mAlarmMessages.Values)
                {
                    //messageid(8)+source(64)+sourcetag(64)+createtime(8)+MessageBody(128)+AppendContent1(64)+AppendContent2(64)+AppendContent3(64)
                    string str = vv.Id + "," + vv.Server + "," + vv.SourceTag + "," + vv.CreateTime.Ticks + "," + vv.MessageBody + "," + vv.AppendContent1 + "," + vv.AppendContent2 + "," + vv.AppendContent3+"\r\n";
                    bs.Write(Encoding.UTF8.GetBytes(str));
                }
            }

            var mcount = mAlarmMessages.Count;

            var mcdatasize = mCompressBuffer.Position;

            long datacount = mcdatasize + mcount * 104 + 8;

            mDataPointer = Marshal.AllocHGlobal((int)datacount);
            DataSize = (int)datacount;

            int offset = 0;
            //写入消息条数
            MemoryHelper.WriteInt32((void*)mDataPointer, offset, mcount);
            offset += 4;

            //写入压缩数据大小
            MemoryHelper.WriteInt32((void*)mDataPointer, offset, (int)(mcdatasize));
            offset += 4;

            //写入压缩数据
            //mCompressBuffer.Position = 0;
            Marshal.Copy(mCompressBuffer.GetBuffer(), 0, mDataPointer + offset, (int)mcdatasize);

            offset += (int)mcdatasize;

            //写入删除信息
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.DeleteTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteNote == null ? "" : vv.DeleteNote, 64);
                offset += 64;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteUser == null ? "" : vv.DeleteUser, 32);
                offset += 32;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateChanged(System.IO.Stream stream)
        {
            var pos = stream.Position;
            if (IsDeleteDirty)
            {
                stream.Position = pos + DeleteOffset;
                UpdateDeleteChanged(stream);
                IsDeleteDirty = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        private unsafe void UpdateDeleteChanged(System.IO.Stream stream)
        {
            var datasize = mAlarmMessages.Values.Count * 104;
            var mDataPointer = Marshal.AllocHGlobal((int)datasize);
            new Span<byte>((void*)mDataPointer, datasize).Fill(0);

            int offset = 0;
            foreach (var vv in mAlarmMessages.Values)
            {
                MemoryHelper.WriteInt64((void*)mDataPointer, offset, vv.DeleteTime.Ticks);
                offset += 8;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteNote == null ? "" : vv.DeleteNote, 64);
                offset += 64;
                MemoryHelper.WriteString((void*)mDataPointer, offset, vv.DeleteUser == null ? "" : vv.DeleteUser, 32);
                offset += 32;
            }

            using (System.IO.UnmanagedMemoryStream ss = new System.IO.UnmanagedMemoryStream((byte*)mDataPointer, datasize))
            {
                ss.CopyTo(stream);
            }
        }
    }
}
