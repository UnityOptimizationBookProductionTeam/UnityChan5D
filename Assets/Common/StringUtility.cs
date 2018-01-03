using System.Text;

namespace Common {

    public static class StringUtility {

        private static StringBuilder _builder = new StringBuilder();

        public static string Add(params string[] args) {
            _builder.Length = 0;

            for (int i = 0, len = args.Length; i < len; ++i) {
                _builder.Append(args[i]);
            }

            return _builder.ToString();
        }
    }
}
