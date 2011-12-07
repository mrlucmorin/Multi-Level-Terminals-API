using System;
using System.Linq;
using System.Collections.Generic;
using Eplan.EplApi.ApplicationFramework;
using Eplan.EplApi.DataModel;
using Eplan.EplApi.HEServices;
using Eplan.EplApi.DataModel.EObjects;

namespace Eplan.EplAddin.MLTReorder.API
{

    /// <summary>
    /// This class implements a EPLAN action.  The Action will register the Addins in that  <seealso cref="IEplAddIn.OnRegister"/> Registerst.
    /// <seealso cref="Eplan::EplApi::ApplicationFramework::IEplAction"/> 
    /// </summary>
    public class MLTReorderAction : IEplAction
    {
        /// <summary>
        /// Execution of the Action.  
        /// </summary>
        /// <returns>True:  Execution of the Action was successful</returns>
        public bool Execute(ActionCallingContext ctx)
        {

            using (UndoStep undo = new UndoManager().CreateUndoStep())
            {
                SelectionSet sel = new SelectionSet();

                //Make sure that terminals are ordered by designation
                var terminals = sel.Selection.OfType<Terminal>().OrderBy(t => t.Properties.FUNC_PINORTERMINALNUMBER.ToInt());
                if (terminals.Count() < 2)
                    throw new Exception("Must select at least 2 Terminals");

                //Create a list of terminals for each level. Those lists get added to a dictionnary
                //with the level as the key.
                Dictionary<int, List<Terminal>> levels = new Dictionary<int, List<Terminal>>();

                //Fill the dictionnary
                foreach (Terminal t in terminals)
                {
                    int level = t.Properties.FUNC_TERMINALLEVEL;

                    if (!levels.ContainsKey(level))
                        levels.Add(level, new List<Terminal>());

                    levels[level].Add(t);
                }

                var keys = levels.Keys.OrderBy(k => k);

                //Make sure that all levels have the same number of terminals
                int qty = levels.First().Value.Count;
                if (!levels.All(l => l.Value.Count == qty))
                    throw new Exception("There must be the same number of Terminals on each level");

                //Assign sort code by taking a terminal from each level in sequence
                int sortCode = 1;
                for (int i = 0; i < qty; i++)
                {
                    foreach (int j in keys)
                    {
                        levels[j][i].Properties.FUNC_TERMINALSORTCODE = sortCode++;
                    }
                }

            }

            return true;
        }
        /// <summary>
        /// Function is called through the ApplicationFramework
        /// </summary>
        /// <param name="Name">Under this name, this Action in the system is registered</param>
        /// <param name="Ordinal">With this overload priority, this Action is registered</param>
        /// <returns>true: the return parameters are valid</returns>
        public bool OnRegister(ref string Name, ref int Ordinal)
        {
            Name = "MLTReorderAction";
            Ordinal = 20;
            return true;
        }

        /// <summary>
        /// Documentation function for the Action; is called of the system as required 
        /// Bescheibungstext delivers for the Action itself and if the Action String-parameters ("Kommandozeile")
        /// also name and description of the single parameters evaluates
        /// </summary>
        /// <param name="actionProperties"> This object must be filled with the information of the Action.</param>
        public void GetActionProperties(ref ActionProperties actionProperties)
        {
            // Description 1st parameter
            // ActionParameterProperties firstParam= new ActionParameterProperties();
            // firstParam.set("Param1", "1. Parameter for MLTReorderAction"); 
            // actionProperties.addParameter(firstParam);

            // Description 2nd parameter
            // ActionParameterProperties firstParam= new ActionParameterProperties();
            // firstParam.set("Param2", "2. Parameter for MLTReorderAction"); 
            // actionProperties.addParameter(firstParam);

        }

    }

}