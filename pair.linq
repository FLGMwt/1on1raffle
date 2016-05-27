<Query Kind="Program" />

void Main()
{
	var NUMBER_OF_ATTEMPTS = 100;
	var emailsPath = @"C:\Code\1on1\emails.txt";
	var emails = File.ReadAllLines(emailsPath).Distinct();
	
	var partners = emails.Select(email => {
		var fullName = GetNameFromEmail(email);
		return new Person
		{
			Email = email,
			FullName = fullName,
			FirstName = fullName.Split(' ')[0]
		};
	}).ToList();
	var extraPerson = partners[0];
	if (partners.Count % 2 != 0)
	{
		partners.Add(extraPerson);
	}

	List<PairSet> pairSets = new List<PairSet>();
	var random = new Random();
	for (int i = 0; i < NUMBER_OF_ATTEMPTS; i++)
	{
		var shuffledPartners = DumbShuffle(partners, random);
		pairSets.Add(GetNewPairSet(shuffledPartners, extraPerson.Email));
	}
	pairSets.Count(a => a.PairingScore == int.MinValue).Dump();
	var bestPairSet = pairSets.OrderBy(pairSet => pairSet.PairingScore).First();
	var pairLines = bestPairSet.Pairs.Select(a => a.FirstPerson.FullName + " & " + a.SecondPerson.FullName)
	File.WriteAllLines(emailsPath + ".pairs", pairLines);
}

public PairSet GetNewPairSet(List<Person> people, string extraPersonEmail)
{
	var pairs = new List<Pair>();
	
	for (int i = 0; i < people.Count; i += 2)
	{
		var pair = new Pair
		{
			FirstPerson = people[i],
			SecondPerson = people[i+1]
		};
		
		switch (pair.FirstPerson.Email.CompareTo(pair.SecondPerson.Email))
		{
			case 0:
				return new PairSet { PairingScore = int.MinValue };
			case 1:
				pair.Key = pair.SecondPerson.Email + pair.FirstPerson.Email;
				break;
			case -1:
				pair.Key = pair.FirstPerson.Email + pair.SecondPerson.Email;
				break;
			default:
				throw new Exception("Compare should only be -1,0, or 1");
		}
		pairs.Add(pair);
	}
	var score = GetPairingScore(pairs);
	return new PairSet { PairingScore = score, Pairs = pairs };
}

public int GetPairingScore(List<Pair> pairs)
{
	return 1;
}

public class PairSet
{
	public ICollection<Pair> Pairs { get; set; }
	public int PairingScore { get; set; }
}

public class Pair
{
	public string Key { get; set; }
	public Person FirstPerson { get; set; }
	public Person SecondPerson { get; set; }
}

public class Person
{
	public string Email { get; set; }
	public string FullName { get; set; }
	public string FirstName { get; set; }
}

public static List<Person> DumbShuffle(IEnumerable<Person> collection, Random random)
{
	return (from item in collection
			let key = random.Next()
			orderby key
			select item).ToList();
}

string GetNameFromEmail(string email)
{
	var namePart = email.ToLower().Substring(0,email.IndexOf('@'));
	var nameSections = namePart.Split('.');
	var correctedNameSections = new List<string>();
	foreach (var nameSection in nameSections)
	{
		// oh hey, Conor
		var hypenatedParts = nameSection.Split('-');
		var correctedHypenParts = hypenatedParts.Select(a => char.ToUpper(a[0]) + a.Substring(1));
		var correctedSection = string.Join("-", correctedHypenParts.ToList());
		correctedNameSections.Add(correctedSection);
	}
	var correctedName = string.Join(" ", correctedNameSections);
	return correctedName;
}