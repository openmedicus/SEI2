using OpenMedicus.WebService.SEI2;

namespace OpenMedicus.SEI2
{
	public class AbortHelper
	{
		public AbortHelper (PatientData patientData, EpisodeData episodeData)
		{
			Patient = patientData;
			Episode = episodeData;
		}
		
		public bool Generate (AbortReport report, OrganizationUnitData oud)
		{
			report.schemaPersonCivilRegistrationIdentifier = Patient.CPR;
			report.city = Patient.City;
			
			return true;
		}
	}
}
