namespace Easy2D.Schedulers
{
    public class GPUSched : Scheduler
    {
        private static GPUSched instance;
        public static GPUSched Instance
        {
            get
            {
                if (instance == null)
                    instance = new GPUSched();

                return instance;
            }
        }
    }
}
