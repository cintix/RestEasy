using System.Text;
namespace RestEasy.IO {
    public static class BinaryUtil {

        private readonly static int[] Empty = Array.Empty<int>();
        public static int[] Locate(this byte[] self, byte[] candidate){
            return Locate(self, candidate, 0);
        }
        public static byte[] GetRange(this byte[] self, int offset, int length){
            byte[] tmp = new byte[length];
            Array.Copy(self,offset,tmp,0,length);
            return tmp;
        }
        public static string RangeToString(this byte[] self, int offset, int length){
            byte[] tmp = new byte[length];
            Array.Copy(self,offset,tmp,0,length);
            return Encoding.UTF8.GetString(tmp);
        }
        
        public static int[] Locate(this byte[] self, string candidate, int offset){
            return Locate(self, Encoding.UTF8.GetBytes(candidate), offset);
        }
        public static int[] Locate(this byte[] self, byte[] candidate, int offset){
            if (IsEmptyLocate(self, candidate)) return Empty;

            List<int> list = new List<int>();
            int maxSize = self.Length;
            int searchSize = candidate.Length;
            for (int index = offset; index < maxSize; index = index + 2) {
                if (searchSize > (maxSize - index)) break;
                if (!IsMatch(self, index, candidate)) {
                    if (self[index + 1] == candidate[0]) index--;
                    continue;
                }
                list.Add(index);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }
        private static bool IsMatch(byte[] array, int position, byte[] candidate){
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        private static bool IsEmptyLocate(byte[] array, byte[] candidate){
            return array == null
                   || candidate == null
                   || array.Length == 0
                   || candidate.Length == 0
                   || candidate.Length > array.Length;
        }
    }
}