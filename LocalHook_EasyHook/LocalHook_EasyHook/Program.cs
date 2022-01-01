using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LocalHook_EasyHook
{
    class Program
    {
        // The matching delegate for MessageBeep
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate bool MessageBeepDelegate(uint uType);

        // Import the method so we can call it
        [DllImport("user32.dll")]
        static extern bool MessageBeep(uint uType);

        static private bool MessageBeepHook(uint uType)
        {
            // We aren't going to call the original at all
            // but we could using: return MessageBeep(uType);
            Console.WriteLine("...intercepted...");
            Console.WriteLine("uType: " + (BeepType)uType);

            return false;
        }

        static private void PlayMessageBeep()
        {
            Console.Write("    MessageBeep(BeepType.Asterisk) return value: ");
            Console.WriteLine(MessageBeep((uint)BeepType.Asterisk));
        }



        static void Main(string[] args)
        {
            Console.WriteLine("Calling MessageBeep with no hook.");
            PlayMessageBeep();

            Console.Write("\nPress <enter> to call MessageBeep while hooked by MessageBeepHook:");
            Console.ReadLine();

            Console.WriteLine("\nInstalling local hook for user32!MessageBeep");
            // Create the local hook using our MessageBeepDelegate and MessageBeepHook function
            using (var hook = EasyHook.LocalHook.Create(
                    EasyHook.LocalHook.GetProcAddress("user32.dll", "MessageBeep"),
                    new MessageBeepDelegate(MessageBeepHook),
                    null))
            {
                // Only hook this thread (threadId == 0 == GetCurrentThreadId)
                hook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                PlayMessageBeep();

                Console.Write("\nPress <enter> to disable hook for current thread:");
                Console.ReadLine();
                Console.WriteLine("\nDisabling hook for current thread.");
                // Exclude this thread (threadId == 0 == GetCurrentThreadId)
                hook.ThreadACL.SetExclusiveACL(new int[] { 0 });
                PlayMessageBeep();

                Console.Write("\nPress <enter> to uninstall hook and exit.");
                Console.ReadLine();
            } // hook.Dispose() will uninstall the hook for us
        }


        public enum BeepType : uint
        {          
            SimpleBeep = 0xFFFFFFFF,
            OK = 0x00,
            Question = 0x20,
            Exclamation = 0x30,
            Asterisk = 0x40,
        }



    }
}
