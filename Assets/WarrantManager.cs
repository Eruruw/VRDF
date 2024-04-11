using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class WarrantManager : MonoBehaviour
{
    public TextMeshProUGUI WarrantTextOutput;
    public List<string> DeskNames;
 
    private string caseNumber { get; set; }
    private string date { get; set; }
    private string exactAddress { get; set; }
    public string exactDesk { get; private set; }
    public string typeOfCrime { get; private set; }
    public List<string> validEvidenceList { get; private set; }
    private string typeOfEvidence { get; set; }
    private string day { get; set; }
    private string month { get; set; }
    private string year { get; set; }

    enum CrimeType {Cybercrime, Identity_Theft, Financial_Fraud}
    enum EvidenceType {Digital, Personal, Documentary}
    Dictionary<CrimeType, List<EvidenceType>> crimeToEvidenceMap;

    private void GenerateWarrantParameters()
    {
        DateTime currentDate = DateTime.Now;

        //Convert Enum to array
        CrimeType[] crimeTypes = (CrimeType[])Enum.GetValues(typeof(CrimeType));

        //Get random crime from CrimeType
        CrimeType generatedCrime = crimeTypes[new System.Random().Next(crimeTypes.Length)];

        //Get Evidence List from Generated Crime
        List<EvidenceType> associatedEvidence = crimeToEvidenceMap[generatedCrime];

        //Set string Parameters for Warrant
        caseNumber = (UnityEngine.Random.Range(1000, 10000)).ToString();
        date = currentDate.ToShortDateString();
        exactAddress = "2314 Cyber Lane, Techville, Louisiana";
        exactDesk =  DeskNames[new System.Random().Next(DeskNames.Count)];
        typeOfEvidence = string.Join(" and ",associatedEvidence);
        typeOfCrime = generatedCrime.ToString().Replace("_"," ");
        day = currentDate.DayOfWeek.ToString();
        month = currentDate.ToString("MMMM");
        year = currentDate.Year.ToString();

        validEvidenceList = new List<string>(typeOfEvidence.Split(" and "));
        
    }
    // Build the warrant text using string interpolation
    private string GenerateWarrantText()
    {
        return $@"
VRDF Superior Court
County of VRDF

Case Number: {caseNumber}

To Any Peace Officer of VRDF:

Proof by affidavit having been made this day to me by Officer Harrison, who has reason to believe that on the premises known as {exactAddress}, in the Parish of Lincoln, there is now being concealed certain property, namely {typeOfEvidence}, which is evidence in an ongoing criminal investigation concerning {typeOfCrime}.

And, whereas, said affidavit has established probable cause to believe that the property so described is being concealed on the premises above described and that the foregoing grounds for application for search warrant exist:

YOU ARE HEREBY COMMANDED to search on or before {date}, {exactDesk}'s desk located at {exactAddress} for the property specified and, if found, to seize it, leaving a copy of this warrant and receipt for the property taken and prepare a written inventory of the seized property and promptly return this warrant to Judge Elizabeth Smith at the Louisiana Superior Court of Lincoln Parish.

Given under my hand and dated this {day} of {month}, {year}.

[Digital Signature Line for Judge]

Judge Elizabeth Smith
Judge of the Superior Court
";
}

    // Start is called before the first frame update
    void Start()
    {
        if (WarrantTextOutput != null)
        {
            crimeToEvidenceMap = new Dictionary<CrimeType, List<EvidenceType>>
            {
            {CrimeType.Cybercrime, new List<EvidenceType> {EvidenceType.Digital}},
            {CrimeType.Identity_Theft, new List<EvidenceType> {EvidenceType.Digital, EvidenceType.Personal}},
            {CrimeType.Financial_Fraud, new List<EvidenceType> {EvidenceType.Digital, EvidenceType.Documentary}},
            // Add other crimes and their associated evidence types here
            };

            GenerateWarrantParameters();
            WarrantTextOutput.text = GenerateWarrantText();
        }
        else
        {
            Debug.LogError("TextMeshPro component not attached to the script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
