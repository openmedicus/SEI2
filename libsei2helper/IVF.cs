<<<<<<< HEAD
﻿using OpenMedicus.Data;
using OpenMedicus.WebService.SEI2;
using OpenMedicus.WebService.SEI2ReportProxy;
=======
﻿using Dynamics.AX.Application;

using Microsoft.Dynamics.Ax.Xpp;

using OpenMedicus.Data;
using OpenMedicus.WebService.SEI2;
>>>>>>> f75ffd7ecab1c6be3c504745baf28f0dd344992c

namespace OpenMedicus.SEI2
{
	public class IVFHelper
	{
		//public IvfData Data { get; set; }
		public IvfAspirationData AspirationData { get; set; }
		public IvfComplicationData ComplicationData { get; set; }
		public IvfInformationData InformationData { get; set; }
		public IvfMedicamentalDataCollection MedicamentalData { get; set; }
		public IvfOocytterDataCollection OocytterData { get; set; }
		public IvfPregnancyData PregnancyData { get; set; }
		public IvfTransfereringData TransfereringData { get; set; }
		public IvfIUIData IUIData { get; set; }

		public IVFHelper () { }
		
		public IVFHelper (IvfData data)
		{
			AspirationData = data.Aspiration;
			ComplicationData = data.Complication;
			InformationData = data.Info;
			MedicamentalData = data.Medicamentals;
			OocytterData = data.Oocytters;
			PregnancyData = data.Pregnancy;
			TransfereringData = data.Transferering;
			IUIData = data.IUI;
		}
		
		public bool Generate (IVFReport report, OrganizationUnitData oud)
		{
			GenerateInformationData (report, InformationData, oud);
			GenerateAspirationData (report, AspirationData);
			GenerateComplicationData (report, ComplicationData);
			GenerateMedicamentalData (report, MedicamentalData);
			GenerateOocytterData (report, OocytterData);
			GeneratePregnancyData (report, PregnancyData);
			GenerateTransfereringData (report, TransfereringData);
			GenerateIUIData (report, IUIData);
			
			return false;
		}

		void GenerateAspirationData (IVFReport report, IvfAspirationData data)
		{
			// txAspirationAntalOocytter
			report.aspirationNumber = data.Count;

			// dtAspiration
			report.apspirationDate = data.Aspiration;
			
			// dtAspirationAflyst
			report.apspirationCancelDate = data.AspirationAflyst;
			
			// bBehandlingsForloebAfsluttet
			report.aspirationEndingofTeatment = data.Closed ? NoYes.Yes : NoYes.No;
			
			// txAspirationstype
			if (data.Aspirationstype == IvfAspirationData.AspirationType.Transvaginal_Modne_Follikler) {
				report.aspirationTypeCode = "KLAA10A";
			} else if (data.Aspirationstype == IvfAspirationData.AspirationType.Transvaginal_Umodne_Follikler) {
				report.aspirationTypeCode = "KLAA10B";
			}
		}

		void GenerateComplicationData (IVFReport report, IvfComplicationData data)
		{
			// iKomplikationer
			report.noComplication = data.Komplikationer <= 0 ? NoYes.Yes : NoYes.No;
			
			// txKomplikationer
			report.complicationDiagnoseCode = data.Kode;
			
			// dtDiagnoseDato
			report.complicationDiagnoseDate = data.DiagnoeDato;
			
			//report.complicationWithHospitalization = 
			//report.complicationWithoutHospitalization =	
		}

		void GenerateMedicamentalData (IVFReport report, IvfMedicamentalDataCollection list)
		{
			// TODO: txPreparat
			//report.
		}

		void GenerateOocytterData (IVFReport report, IvfOocytterDataCollection list)
		{
			// txAnvendelse
			report.iVFUseOfOocytes = new SEI2SchemaIVFUseOfOocytesContract[list.Count];

			for (var i = 0; i < list.Count; i++)
			{
				IvfOocytterData data = list[i];

				report.iVFUseOfOocytes[i] = new SEI2SchemaIVFUseOfOocytesContract
				{
					oocytesNumber = data.Antal,
					supplementCode = data.Kode
				};
			}
		}
		
		void GeneratePregnancyData (IVFReport report, IvfPregnancyData data)
		{
			// dtGraviditetsProeveDato
			report.pregnancySampleDate = data.FosterDato;
			
			// TODO: txHCG
			if (data.HCG <= 0)
				report.pregnancySampleuhCGshCG = SEI2IVFPregnancySampleuhCGshCG.NotSelected;
			else
				report.pregnancySampleuhCGshCG = (data.HCG == 1) ? SEI2IVFPregnancySampleuhCGshCG.shCG : SEI2IVFPregnancySampleuhCGshCG.uhCG;
			
			// dtBehandlingsAfslutningsDato
			report.pregnancySampleNoPregnancyDateEnded = data.AbortAfslutningDato;
			
			// bIngenProeve
			report.pregnancySampleNotTaken = (data.NoPregnancyTest) ? NoYes.Yes : NoYes.No;
			
			// TODO: iULGraviditet
			// bULIntrauterinGraviditet
			// bULEkstrauterinGraviditet
			//report.pregnancyState =
			
			// dtULIngenSynligGraviditetDato
			
			// dtULSynligGraviditetDato
			
			// bULIntrauterinGraviditet
			report.intrauterinPregnancy = (data.IntrauterinPregnancy) ? NoYes.Yes : NoYes.No;
			
			// TODO: bULEkstrauterinGraviditet
			report.ectopicPregnancy = (data.EkstrauterinPregnancy) ? NoYes.Yes : NoYes.No;
			
			// txULIntraMedHjerteLydAntal
			report.numberWithHeartSound = data.IntrauterinWithHeartbeat;
			
			// txULIntraUdenHjerteLydAntal
			report.numberWithoutHeartSound = data.IntrauterinWithoutHeartbeat;
		}
		
		void GenerateTransfereringData (IVFReport report, IvfTransfereringData data)
		{
			// dtTransferering
			report.transferingDate = data.Transferering;
			
			// dtTransfereringAflyst
			report.transferingCancelDate = data.TransfereringAflyst;
			
			// TODO: txTransAntalFriskeEmbryoner
			//report.Em
			
			// TODO: txTransferFriskeElektivt
			
			// TODO: bTransferFriskeFertiliseret1
			
			// TODO: bTransferFriskeFertiliseret2
			
			// TODO: bTransferFriskeFertiliseret3
			
			// TODO: txTransAntalOptoeedeEmbryoner
			
			// TODO: txTransferOptoeedeElektivt
			
			// TODO: bTransferOptoeedeFertiliseret1
			
			// TODO: bTransferOptoeedeFertiliseret2
			
			// TODO: bTransferOptoeedeFertiliseret3
			
			// TODO: iOverskydendeEmbryoner
			
			// TODO: txOvrskydEmbryAntalCryoSlow
			
			// TODO: txOvrskydEmbryAntalCryoVitri
			
			// TODO: txOvrskydEmbryAntalDoneret
			
			// TODO: bAssistedHatching
			report.performedAssistedHatching = data.AssistedHatching ? NoYes.Yes : NoYes.No;
		}
		
		void GenerateIUIData (IVFReport report, IvfIUIData data)
		{
			// dtIUIH
			report.iUIHDate = data.IUIH;

			// dtIUID
			report.iUIDDate = data.IUID;

			// dtIUIAflyst
			report.iUIHIHIDCancelDate = data.IUIAflyst;

			// TODO: txIUIKode
		}

		void GenerateInformationData (IVFReport report, IvfInformationData data, OrganizationUnitData oud)
		{
			// dtBehandlingStart
			report.treatmentDate = data.Created;
			
			// txKlinikSygehus
			if (!string.IsNullOrWhiteSpace (oud.SGH))
			{
				report.hospitalCode = oud.SGH;
			}

			// iKlinikYdernummer
			
			// txKvindeCPR
			report.schemaPersonCivilRegistrationIdentifier = data.KvindeCPR;
			
			// txKvindeKommune
			report.municipalityCode = $"{data.KvindeKommune:000}";
			
			// txKvindeHojde
			report.height = data.KvindeHojde;
			
			// txKvindeVaegt
			report.weight = data.KvindeVaegt;
			
			// txKvindeRygning
			report.smokingFrequency = data.KvindeRygning;
			
			// bKvindeRygningVilIkkeOplyse
			report.smokingWillNoInform = data.KvindeRygningIkkeOplyse ? NoYes.Yes : NoYes.No;
			
			// txKvindeAlkohol
			report.alcoholFrequency = data.KvindeAlkohol;
			
			// bKvindeAlkoholVilIkkeOplyse
			report.alcoholWillNotInform = data.KvindeAlkoholIkkeOplyse ? NoYes.Yes : NoYes.No;

			// bKvindeEnlig
			report.singleLiving = data.KvindeEnlig ? NoYes.Yes : NoYes.No;
			
			// txPartnerCPR
			report.partnerCivilRegistrationIdentifier = data.PartnerCPR;
			
			// txPartnerHojde
			report.partnerHeight = data.PartnerHeight;
			
			// txPartnerVaegt
			report.partnerWeight = data.PartnerWeight;
			
			// txPartnerRygning
			report.partnerSmokingFrequency = data.PartnerRygning;
			
			// bPartnerRygningVilIkkeOplyse
			report.partnerSmokingWillNoInform = data.PartnerRygningIkkeOplyse ? NoYes.Yes : NoYes.No;
			
			// txPartnerAlkohol
			report.partnerAlcoholFrequency = data.PartnerAlkohol;
			
			// bPartnerRygningVilIkkeOplyse
			report.partnerAlcoholWillNotInform = data.PartnerAlkoholIkkeOplyse ? NoYes.Yes : NoYes.No;
			
			// iBarnloesSidenAar
			report.involuntaryInfertileSinceYear = data.ChildlessSince;
			
			// TODO: iAarsagTilBarnloeshed
			report.causeOfInfertilityTreatment = (data.ChildlessReason == 0 && data.KvindePrimaer != null) ? SEI2IVFCauseOfInfertilityTreatment.Female : SEI2IVFCauseOfInfertilityTreatment.NotSelected;
			
			// txPGD
			if (data.PGD != null)
				report.causeOfInfertilityTreatment = SEI2IVFCauseOfInfertilityTreatment.PGD;

			// txKvindePrimaer
			if (data.ChildlessReason == 0 && data.KvindePrimaer != null)
				report.womanPrimaryCauseCode = data.KvindePrimaer;
			
			// txKvindeSekundaer
			if (data.ChildlessReason == 0 && data.KvindeSekundaer != null)
				report.womanPrimaryCauseSupplementCode = data.KvindeSekundaer;
			
			// txMandPrimaer
			if (data.ChildlessReason == 0 && data.MandPrimaer != null)
				report.manPrimaryCauseCode = data.MandPrimaer;
			
			// txMandSekundaer
			if (data.ChildlessReason == 0 && data.PartnerSecondary != null)
				report.manPrimaryCauseSupplementCode = data.PartnerSecondary;
			
			// txFamilieanamneseSygdom
			report.pGDFamilySignessCode = data.FamilieanamneseSygdom;
			
			// TODO: bBehandlingFriskeEmbryoner
               //         report. = data.TreatmentEmbryon ? "true" : "false";

                        // TODO: bBehandlingFriskeEmbryonIVF1");
                 //       report. = data.TreatmentEmbryonIVF1 ? "true" : "false";

                        // TODO: bBehandlingFriskeEmbryonIVF2");
                        //report. = data.TreatmentEmbryonIVF2 ? "true" : "false";

                        // TODO: bBehandlingFriskeEmbryonICSI1");
                        //report. = data.TreatmentEmbryonICSI1 ? "true" : "false";

                        // TODO: bBehandlingFriskeEmbryonICSI2");
                        //report. = data.TreatmentEmbryonICSI2 ? "true" : "false";

                        // TODO: bBehandlingFriskeEmbryonICSI3");
                        //report. = data.TreatmentEmbryonICSI3 ? "true" : "false";

                        // TODO: bBehandlingFriskeEmbryonIngen");
                        //report. = data.TreatmentEmbryonNone ? "true" : "false";

                        // TODO: bBehandlingOptoeedeEmbryoner");
                        //report. = data.TreatmentDeEmbryon ? "true" : "false";

                        // TODO: bBehandlingOptoeEmbryonIVF1");
                        //report. = data.TreatmentDeEmbryonIVF1 ? "true" : "false";

                        // TODO: bBehandlingOptoeEmbryonIVF2");
                        //report. = data.TreatmentDeEmbryonIVF2 ? "true" : "false";

                        // TODO: bBehandlingOptoeEmbryonICSI1");
                        //report. = data.TreatmentDeEmbryonICSI1 ? "true" : "false";

                        // TODO: bBehandlingOptoeEmbryonICSI2");
                        //report. = data.TreatmentDeEmbryonICSI2 ? "true" : "false";

						// TODO: bBehandlingOptoeEmbryonICSI3");
                        //report. = data.TreatmentDeEmbryonICSI3 ? "true" : "false";

                        // TODO: iBehandlingOptoeEmbryonNedfrys");
                        //report. = data.DefrostEmbryon.ToString ();

                        // TODO: bBehandlingOocytter");
                        //report. = data.TreatmentOocyt ? "true" : "false";

                        // TODO: bOocytIVF1");
                        //report. = data.TreatmentOocytIVF1 ? "true" : "false";

                        // TODO: bOocytIVF2");
                        //report. = data.TreatmentOocytIVF2 ? "true" : "false";

                        // TODO: bOocytICSI1");
                        //report. = data.TreatmentOocytICSI1 ? "true" : "false";

                        // TODO: bOocytICSI2");
                        //report. = data.TreatmentOocytICSI2 ? "true" : "false";

                        // TODO: bOocytICSI3");
                        //report. = data.TreatmentOocytICSI3 ? "true" : "false";

                        // TODO: bOocytIngen");
                        //report.Oocy = data.TreatmentOocytNone ? "true" : "false";

                        // TODO: iOocytKilde
                        report.methodOfFertilizationOocytesSource = (data.OocytSource == 0) ? SEI2IVFMethodOfFertilizationOocytesSource.DonatedOocytes : SEI2IVFMethodOfFertilizationOocytesSource.OwnOocytes;

                        // TODO: iOocytDonor
                        report.methodOfFertilizationOocytesDonor = (data.OocytDonor == 0) ? SEI2IVFMethodOfFertilizationOocytesDonor.Fertile : SEI2IVFMethodOfFertilizationOocytesDonor.IVF;

                        // TODO: iOocytType
                        report.methodOfFertilizationOocytesType = (data.OocytType == 0) ? SEI2IVFMethodOfFertilizationOocytesType.FreshOocytes : SEI2IVFMethodOfFertilizationOocytesType.NotSelected;

                        // TODO: bBehandlingIUI
                        report.iUIHorIUID = data.TreatmentIUI ? NoYes.Yes : NoYes.No;

						// TODO: iSaedType
						report.iUIHorIUIDSpermType = (data.SemenType == 0) ? SEI2IVFIUIHorIUIDSpermType.IUID : SEI2IVFIUIHorIUIDSpermType.IUIH;

                        // TODO: bDistribution
                        report.distribution = data.Distribution ? NoYes.Yes : NoYes.No;

                        // TODO: iImportType
                        //report. = data.ImportType.ToString ();

                        // TODO: txImportAntal
                        if (data.Distribution) {
                                //report.d = String.Format ("VPH{0:000}", data.ImportCount);
                        }

                        // TODO: txImportFra
                        if (data.ImportFrom > 0) {
                                //report. = (data.ImportFrom == 1) ? "klinik" : "land";
                        }

                        // TODO: iEksportType
                        //report. = data.ExportType.ToString ();

                        // TODO: txEksportAntal
                        if (data.ExportType > 0) {
                                //report. = String.Format ("VPH{0:000}", data.ExportCount);
                        }

                        // TODO: txEksportTil
                        if (data.ExportTo > 0) {
                                //report. = (data.ExportTo == 1) ? "klinik" : "land";
                        }

                        // txIndberetningsStatus NOT USED
                        // txValideringsStatus NOT USED

		}
	}
}
