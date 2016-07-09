using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace RandomMaster
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FeatureTest : Page
    {
        public FeatureTest()
        {
            this.InitializeComponent();
        }

        private void testInput_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                startBtn_Click(null, null);
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if (testInput.Text == "")
            {
                testOutput.Text = "Please Input something...";
                return;
            }
            if (Config.IsInternet())
            {
				NLP.Word[] words;
				try
				{
					var task = NLP.AnalyzeQues(testInput.Text);
					task.Wait();
					words = task.Result;
				}
				catch (Exception ee)
				{
					testOutput.Text = ee.Message;
					return;
				}

                string[] feature = NLP.GetFeature(words);
                var ques = testInput.Text;

                string bestQues = "";
                double score = 0;

                if (feature == null)
                {
                    string[] coreResult = getQuesCore(words).Split();
                    string category = coreResult[0];
                    string core = coreResult[1];
                    testOutput.Text = "Syntax Error..." + "\n" +
                                     "Category: " + category + "\n" +
                                     "Core: " + core;
                }
                else
				{
					var qm = new QuesManager();
					var queses = qm.queses;

					string[] bestFeat = new string[4];
                    double[] scores = new double[5];
                    double[] maxScores = new double[5];
                    double[] facotrs = new double[] { 0.25, 0.45, 0.1, 0.1, 0.1 };
                    var maxScore = 0.0;
                    foreach (var q in queses)
                    {
                        score = 0.0;
                        if (q.feature != null)
                        {
                            var i = 0;
                            for (i = 0; i < 4; i++)
                            {
                                scores[i] = NLP.SimiScore(feature[i], q.feature[i]);
                                score += scores[i] * facotrs[i];
                            }

                            scores[i] = NLP.SimiScore(ques, q.question);
                            score += scores[i] * facotrs[i];
                        }
                        else
                        {
                            // The question has not got feature
                            score = NLP.SimiScore(ques, q.question);
                        }

                        if (score > maxScore)
                        {
                            maxScore = score;
                            bestQues = q.question;
                            for (int i = 0; i < 4; ++i)
                            {
                                maxScores[i] = scores[i];
                                bestFeat[i] = q.feature[i];
                            }
                            maxScores[4] = scores[4];
                        }
                    }
                    //string[] coreResult = getQuesCore(words).Split();
                    var coreResult = new string[2];
                    string category = coreResult[0];
                    string core = coreResult[1];
                    testOutput.Text =
                        feature[0] + " -- " + bestFeat[0] + " -- " + maxScores[0] + "\n" +
                        feature[1] + " -- " + bestFeat[1] + " -- " + maxScores[1] + "\n" +
                        feature[2] + " -- " + bestFeat[2] + " -- " + maxScores[2] + "\n" +
                        feature[3] + " -- " + bestFeat[3] + " -- " + maxScores[3] + "\n" +
                        "Best Match: " + bestQues + " -- " + maxScores[4] + "\n" +
                        "Total Score: " + maxScore + "\n" +
                        "Category: " + category + "\n" +
                        "Core: " + core;
                }
            }
            else
                testOutput.Text = "No Internet Connection...";
        }

        //Get core of question and the category of this ques based on pronouns
        //The category includes: "place", "people", "time", "thing", "unknown".
        private string getQuesCore(NLP.Word[] words)
        {
            string category = "unknown";
            string core = "unknown";
            // detect the number of pronouns, filtering the sentences with multiple pronouns
            int pronounCount = 0;
            int pronounNumber = -1;
            foreach (var word in words)
            {
                if (word.cont == "什么" || word.cont.Contains("哪") || word.cont == "谁" || word.cont.Contains("啥"))
                {
                    pronounCount += 1;
                    pronounNumber = word.id;
                }
            }
            if (pronounCount > 1)
            {
                return category + "" + core;
            }
            // get the core 
            else
            {
                NLP.Word pronoun = words[pronounNumber];
                //determine the category of sentence
                //start from “什么”
                #region "什么" series
                if (pronoun.cont == "什么")
                {
                    //if pronoun's relate is "ATT", it has a parent which may be the core
                    if (pronoun.re == "ATT")
                    {
                        var pronounParent = words[pronoun.pa];
                        //The sentence asks about a place
                        //When asking for a place, we concern about what will user do there, so the action will be the core.
                        #region "什么地方"“什么地点”“什么地儿” Place category
                        if (pronounParent.cont == "地方" || pronounParent.cont == "地点" || pronounParent.cont == "地儿")
                        {
                            category = "place";
                            //Find the action that attributed by this place
                            //Below conclude the situation in which sentence contains a preposition, like "在什么地方见她比较好"
                            if (pronounParent.re == "POB")
                            {
                                var prep = words[pronounParent.pa];
                                if (prep.re == "ADV")
                                {
                                    core = words[prep.pa].cont;
                                }
                            }
                            //Below cover the situation without a preposition like "什么地方见她比较好"
                            else if (pronounParent.re == "ADV")
                            {
                                core = words[pronounParent.pa].cont;
                            }
                            //Below cover the situation that the place acts as a object, like "去什么地方玩"
                            //The COO component should be the core 
                            //Note that the COO component we need is the child node.
                            else if (pronounParent.re == "VOB")
                            {
                                var tempAction = words[pronounParent.pa];
                                try
                                {
                                    var Action = words.First(word => (word.pa == tempAction.id && word.re == "COO"));
                                    core = Action.cont;
                                }
                                catch { }
                            }
                        }
                        #endregion
                        //The sentence asks about a time
                        //When asking for a time, we want to know what will user do that time, so the action still is the core
                        //TODO: In addition, if necessary, we can generate a time randomly for user.Besides that we can also try to do some other reply about time. 
                        #region "什么时间" “什么时候” “什么时刻” “什么点儿”
                        else if (pronounParent.cont == "时间" || pronounParent.cont == "时候" || pronounParent.cont == "时刻" || pronounParent.cont == "点")
                        {
                            category = "time";
                            //Find the action that attributed by this time
                            //Below conclude the situation in which sentence contains a preposition like "在什么时候见她比较好"
                            if (pronounParent.re == "POB")
                            {
                                var prep = words[pronounParent.pa];
                                if (prep.re == "ADV")
                                {
                                    core = words[prep.pa].cont;
                                }
                            }
                            //Below cover the situation without a preposition like "什么时候见她比较好"
                            else if (pronounParent.re == "ADV")
                            {
                                core = words[pronounParent.pa].cont;
                            }
                        }
                        #endregion
                        //The sentence asks for certain person.
                        //When asking for a person, we want to know what matters with this person, so the matter, aka "the action", could be the core.
                        //Besides, what group this person is involved may also help to determine the ques, this can be talked later.
                        //TODO: Group function: user can generate a ques only consist of one word, which will be regarded as the only core and this very ques will also viewed as a group, not a sentence,  whose category is marked as "group"(Maybe its UI is also a little different from normal queses).
                        //When commiting a comparing work, the group will be considerd fisrt, and it should be strictly matched.
                        //Example1: Ques:"同学"  Option:[A list of people] UserAks:"哪位同学去交钱"
                        //Example2: Ques:"口味" Option:[A list of flavors] UserAsks:"吃什么口味的蛋糕？"
                        #region “什么人”
                        else if (pronounParent.cont == "人")
                        {
                            category = "people";
                            //The person is the subject or the object
                            //TODO: A prpblemn may occur when we didn't know whether this person is the object or the subject of the behavior like:
                            //"什么人打我" and "我打什么人". 
                            //That two sentences are all "person" category and the core are all "打", which may cast ambigous problem.
                            if (pronounParent.re == "SBV" || pronounParent.re == "VOB")
                            {
                                core = words[pronounParent.pa].cont;
                            }
                            //The person is the possesser of something involved in the action, in this case, the thing is more important than the the action thus should be the core
                            //Example: "我该拿走什么人的电脑" "我用什么人的电脑来玩游戏" The person is limited by having a computer 
                            //It's complete the same as the previous condition, but the meaning is different.
                            //This logic will also be used for another person case talked about later, but the code will be merged.
                            else if (pronounParent.re == "ATT" && words[pronounParent.pa].cont == "的")
                            {
                                core = words[pronounParent.pa].cont;
                            }

                        }
                        #endregion
                        //The sentence asks about a thing.
                        //When asking for a thing, we focus on what category that thing may belong to, so the category becomes the core
                        //TODO: We don't check what the user do with that thing for now, which can be discussed later.
                        #region “什么XX”
                        else
                        {
                            category = "thing";
                            core = pronounParent.cont;
                        }
                        #endregion
                    }
                    //"什么" is singular. Under that condition, it may host the possibility that the pronoun refers to some certain people or things.
                    //When it refers to a people, the action is the core
                    //When it refers to a thing, we can only regrad the action as the core due to lack of the limitation, i.e the category of that thing,
                    //In summary, find the action.
                    //TODO: But, we still have no clue about exactly a person or a thing it refers,thus it can only be taged as "unknow", which can be talked later.
                    //When matching an "unknow" ques, the "unknow" queses in the list should be considered first.
                    else
                    {
                        category = "unknown";
                        if (pronoun.re == "SBV" || pronoun.re == "VOB")
                        {
                            core = words[pronoun.pa].cont;
                        }
                    }
                }
                #endregion
                //"哪" series
                #region "哪 sereis" 
                if (pronoun.cont.Contains("哪"))
                {
                    #region "哪里" "哪儿" singular "哪" Place category
                    if ((pronoun.cont == "哪里" || pronoun.cont == "哪儿" || pronoun.cont == "哪") && pronoun.re != "ATT")
                    {
                        category = "place";
                        //When asking for a place, the action is the core
                        //The logic has been descirbed above.
                        if (pronoun.re == "POB")
                        {
                            var prep = words[pronoun.pa];
                            if (prep.re == "ADV")
                            {
                                core = words[prep.pa].cont;
                            }
                        }
                        else if (pronoun.re == "ADV")
                        {
                            core = words[pronoun.pa].cont;
                        }
                        else if (pronoun.re == "VOB")
                        {
                            var tempAction = words[pronoun.pa];
                            try
                            {
                                var Action = words.First(word => (word.pa == tempAction.id && word.re == "COO"));
                                core = Action.cont;
                            }
                            catch { }
                        }
                    }
                    #endregion
                    #region "哪XX" Thing categoty
                    //This series countains a quantifier to recognize
                    //The methods of finding the core is find the word that the re is not the 'ATT'
                    else if (pronoun.cont == "哪" && pronoun.re == "ATT")
                    {
                        var temp = pronoun;
                        while (temp.re == "ATT")
                        {
                            temp = words[temp.pa];
                        }
                        core = temp.cont;
                    }
                    #endregion
                }
                #endregion
                //"谁"  "谁的"  Person category
                #region "谁" "谁的"
                else if (pronoun.cont == "谁")
                {
                    category = "people";
                    if (pronoun.cont == "谁")
                    {
                        if (pronoun.re == "SBV" || pronoun.re == "VOB" || (pronoun.re == "ATT" && words[pronoun.pa].cont == "的"))
                        {
                            core = words[pronoun.pa].cont;
                        }
                    }
                }
                #endregion
            }
            return category + " " + core;
        }
    }
}


