using OpenMedicus.Data;
using OpenMedicus.WebService.SEI2;
<<<<<<< HEAD
using OpenMedicus.WebService.SEI2ReportProxy;
=======
>>>>>>> f75ffd7ecab1c6be3c504745baf28f0dd344992c

namespace OpenMedicus.SEI2
{
	public class AbortHelper
	{
		public PatientData Patient { get; } 
		public EpisodeData Episode { get; }
		
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
