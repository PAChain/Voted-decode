using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;

namespace VotedDecode
{
    public class ECBufferedIesCipher : BufferedIesCipher
    {
        private readonly IesEngine engine;
        private bool forEncryption;

        public ECBufferedIesCipher(IesEngine engine) : base(engine)
        {
            this.engine = engine;
        }

        public void Init(bool forEncryption, ICipherParameters privateKey, ICipherParameters publicKey)
        {
            this.forEncryption = forEncryption;
            //base.Init(forEncryption, parameters);
            this.engine.Init(forEncryption, privateKey, publicKey, null);
        }
    }
}
