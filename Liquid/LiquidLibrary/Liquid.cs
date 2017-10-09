using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace Liquid
{
    public class Liquid
    {
        public static readonly ILog log =
            LogManager.GetLogger(typeof(Liquid));
        public bool Verbose { get; set; } = false;
        public Dictionary<int, Plug> PlugList { get; set; } = new Dictionary<int, Plug>();
        public int SidCount { get; set; } = 0;
        public int LinkCount { get; set; } = 0;
        public bool ErrorOnCycle { get; set; } = true;
        public bool CheckCycleOnLink { get; set; } = false;
        
        public int AllocateId( )
        {
            return ++this.SidCount;
        }

        public Plug RetrievePlug (int sid)
        {
            PlugList.TryGetValue(sid, out Plug output);
            return output;
        }

        public void ReindexPlug(Plug plug) //Todo: Should return Plug or void?
        {
            PlugList.TryGetValue(plug.Sid, out Plug output);
            if (output == null)
            {
                throw new UnallocatedPlugException("ReindexPlug called on unallocated sid");
            }
            PlugList[plug.Sid] = plug;
        }

        public static void FreezePlug(Plug plug)
        {
            plug.Frozen = true;
        }
        public static void ThawPlug (Plug plug)
        {
            plug.Frozen = false;
            //Todo plug.Valve.Notify plug
        }

    }
    public class Plug
    {
        public int Sid { get; set; }
        public bool Frozen { get; set; } = false;
        public string Pipe { get; set; }
        public bool Dirty { get; set; }
        public bool Stainless { get; set; }
        public Liquid Parent { get; set; }
        public Valve Valve { get; set; } = new Valve();
        public ICollection<Plug> Observers { get; set; } 
        public ICollection<Plug> Subordinates { get; set; } 
        public ICollection<Object> Liquid { get; set; } //Todo: double check type;

        public Plug(Liquid parent)
        {
            Parent = parent;
        }
    }

    public class ServerPlug: Plug
    {
        public ServerPlug(Liquid parent) : base(parent)
        {
            Parent = parent;
        }
    }

    public class Valve
    {
        public string Type { get; set; }
        public string PipeServerClass { get; set; } //todo: add enum or other Type? 
        public Delegate Setup { get; set; }
        public Delegate Cleanse { get; set; }
        public Delegate OnLink { get; set; }

        public bool CycleExists (Plug plug)
        {
            bool cycle = false;
            //Todo: check cycle
            return cycle;
        }

        public static void Init (Plug plug)
        {
            plug.Sid = plug.Parent.AllocateId();
            plug.Observers = new List<Plug>();
            plug.Subordinates = new List<Plug>();

            if (plug.Pipe == "bridge") { plug.Liquid = new List<Object>(); }  //todo: confirm type of Liquid

            plug.Parent.PlugList.Add(plug.Sid, plug);

            //todo: call setup
            //todo: call cleanse
           
        }

        public static void Destroy(Plug plug)
        {
            //Todo: Find native way to destroy aspects of plug
        }

        public static void Link(Plug Observer, Plug Subordinate)
        {

        }

        /// <summary>
        /// A generalized link querying method. Returns the number of plugs we are observing
        /// </summary>
        /// <param name="plug"></param>
        /// <returns></returns>
        public static int CountLinks(Plug plug)
        {
            return plug.Subordinates.Count();
            //todo: missing label refinement
        }

        public static bool IsLinked(Plug plug)
        {
            return 0 < Valve.CountLinks(plug);
        }
        public static bool IsLinkedWith(Plug plug, Plug subordinate)
        {
            return 0 < plug.Subordinates.Where(p => p == subordinate).Count();
        }

        public static void Insubordinate(Plug plug)
        {
            foreach (var subordinate in plug.Subordinates)
            {
                subordinate.Observers.Remove(plug);
            }
            plug.Subordinates.Clear();
        }

        public static Plug NewPipe(Plug plug)
        {
            Plug pipeServer;
            // Check if there is a Pipe server already
            if (plug.Valve.PipeServerClass == "")
            {
                pipeServer = new Plug(plug.Parent);
                Valve.Init(pipeServer);
                
            } else
            {
                pipeServer = new Plug(plug.Parent);
                pipeServer.Valve.PipeServerClass = plug.Valve.PipeServerClass;
            }
            return pipeServer;
        }
    }
    

    public class UnallocatedPlugException : Exception
    {
        public UnallocatedPlugException(string message) : base(message)
        {
        }
    }
}
