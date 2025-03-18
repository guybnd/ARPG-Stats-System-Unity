using UnityEngine;

public class Proceess : MonoBehaviour
{
    public void CalcSkill(SkillData data)
    {
        if(data != null)
        {
            var skillArray = data.skillName;

            if(skillArray.Length > 0)
            {
                foreach(var v in skillArray)
                {
                    ParseSkill(v);
                }
            }
        }
    }

    private void ParseSkill(string input)
    {
        var parts = input.Split('_');

        if(parts.Length != 2)
        {
            Debug.Log("what are you doing stupid.");
        }
        else
        {
            var key = parts[0];
            var value = parts[1];

            if(key == "damage")
            {
                var damage = ParseInt(value);
            }
        }
    }

    private int ParseInt(string value)
    {
        var defaultValue = -1;

        if(int.TryParse(value, out int result))
        {
            defaultValue = result;
        }

        return defaultValue;
    }
}
