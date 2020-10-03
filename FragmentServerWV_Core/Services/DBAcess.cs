using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FragmentServerWV.Models;
using NHibernate;
using NHibernate.Cfg;

namespace FragmentServerWV.Services
{
    public class DBAcess
    {
        private static DBAcess _instance = null;
        private ISessionFactory _sessionFactory;

        public static DBAcess getInstance()
        {
            if (_instance == null)
            {
                _instance = new DBAcess();
            }

            return _instance;
        }

        public DBAcess()
        {
            var config = new Configuration().Configure();
            config.AddAssembly("FragmentServerWV_Core");
            _sessionFactory = config.BuildSessionFactory();
        }

        public List<BbsCategoryModel> GetListOfBbsCategory()
        {
            List<BbsCategoryModel> categoryList = new List<BbsCategoryModel>();
            using (ISession session = _sessionFactory.OpenSession())
            {
                ICriteria criteria = session.CreateCriteria(typeof(BbsCategoryModel));
                IList<BbsCategoryModel> bbsCategoryModels;
                bbsCategoryModels = session.Query<BbsCategoryModel>().ToList();
                categoryList.AddRange(bbsCategoryModels);
                session.Close();
            }

            return categoryList;
        }


        public List<BbsThreadModel> getThreadsByCategoryID(int categoryID)
        {
            List<BbsThreadModel> threadLists = new List<BbsThreadModel>();

            using (ISession session = _sessionFactory.OpenSession())
            {
                threadLists.AddRange(
                    session.Query<BbsThreadModel>().Where
                            (x => x.categoryID == categoryID)
                        .ToList());
                session.Close();
            }

            return threadLists;
        }


        public List<BbsPostMetaModel> getPostsMetaByThreadID(int threadID)
        {
            List<BbsPostMetaModel> postMetaList = new List<BbsPostMetaModel>();
            using (ISession session = _sessionFactory.OpenSession())
            {
                postMetaList.AddRange(
                    session.Query<BbsPostMetaModel>().Where
                            (x => x.threadID == threadID)
                        .ToList());

                session.Close();
            }

            return postMetaList;
        }


        public BbsPostBody getPostBodyByPostID(int postID)
        {
            BbsPostBody postBody = new BbsPostBody();

            using (ISession session = _sessionFactory.OpenSession())
            {
                postBody = session.Query<BbsPostBody>().Where(x => x.postID == postID).FirstOrDefault();
            }

            return postBody;
        }

        public void createNewPost(byte[] argument, uint u)
        {
            byte[] threadIdBytes = new byte[4];
            byte[] usernameBytes = new byte[16];
            byte[] postTitleBytes = new byte[36];
            byte[] postBodyBytes = new byte[600];

            Buffer.BlockCopy(argument, 0, threadIdBytes, 0, 4);
            Buffer.BlockCopy(argument, 4, usernameBytes, 0, 16);
            Buffer.BlockCopy(argument, 84, postTitleBytes, 0, 32);
            Buffer.BlockCopy(argument, 134, postBodyBytes, 0, 600);

            Console.WriteLine("Thread ID  = " + BitConverter.ToString(threadIdBytes));
            Console.WriteLine("username " + BitConverter.ToString(usernameBytes));
            Console.WriteLine("post Title " + BitConverter.ToString(postTitleBytes));
            Console.WriteLine("post Body " + BitConverter.ToString(postBodyBytes));

            // int threadIDX = BitConverter.ToInt32(threadIdBytes);
            String username = Encoding.ASCII.GetString(usernameBytes);
            String postTitle = Encoding.ASCII.GetString(postTitleBytes);
            String postBody = Encoding.ASCII.GetString(postBodyBytes);

            int threadID = 0;
            if (u == 0)
            {
                // Create a new Thread before posting 

                BbsThreadModel thread = new BbsThreadModel();
                thread.threadTitle = postTitle;
                thread.categoryID = 1;

                using (ISession session = _sessionFactory.OpenSession())
                {
                     threadID = session.Query<BbsThreadModel>().Max(x => (int?) x.threadID).Value + 1;
                     thread.threadID = threadID;
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Save(thread);
                        transaction.Commit();
                        
                    }
                }
            }
            else
            {
                threadID = Convert.ToInt32(u);
            }


            // post to an existing thread 

            BbsPostMetaModel meta = new BbsPostMetaModel();

            
            meta.unk2 = 0;
            meta.date = DateTime.Now;
            meta.username = username;
            meta.title = postTitle;
            meta.subtitle = postTitle.Substring(0, 16);
            meta.unk3 = "unk3";
            meta.threadID = threadID;

            using (ISession session = _sessionFactory.OpenSession())
            {
                int postID = session.Query<BbsPostMetaModel>().Max(x => (int?) x.postID).Value + 1;
                int postBodyID = session.Query<BbsPostBody>().Max(x => (int?) x.postBodyID).Value + 1;
                meta.postID = postID;
                meta.unk0 = postID - 1;
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.Save(meta);
                    //transaction.Commit();

                    BbsPostBody body = new BbsPostBody();
                    body.postBodyID = postBodyID;
                    body.postBody = postBody;
                    body.postID = meta.postID;

                    session.Save(body);
                    transaction.Commit();
                }
            }

            {
            }


            // Console.WriteLine("Thread ID  = " + threadIDX);
            Console.WriteLine("username " + username);
            Console.WriteLine("post Title " + postTitle);
            Console.WriteLine("post Body " + postBody);
        }
    }
}