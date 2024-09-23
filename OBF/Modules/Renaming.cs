using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OBF.Modules;

public static class Renaming
{
    private static readonly Random random = new();
    internal static AssemblyDefinition? OriginalAssembly { get; set; }
    /*internal static void RenameAssembly(AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)
            foreach (var type in module.Types)
                if (!string.IsNullOrEmpty(type.Namespace))
                    type.Namespace = GenerateUniqueName(type.Namespace);

        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            if ((type.Name.Equals("AppStart") || type.Name.Equals("Client")) && type.IsPublic)
                type.Name = GenerateUniqueName(type.Name);

            if (type.IsEnum || type.IsPublic)
                continue;

            type.Name = GenerateUniqueName(type.Name);

            foreach (MethodDefinition field in type.Methods)
            {
                if (!field.HasBody || field.IsVirtual || field.DeclaringType != type)
                    continue;

                if (!field.IsConstructor && !field.IsSpecialName)
                    field.Name = GenerateUniqueName(field.Name);

                foreach (var gen in field.GenericParameters)
                    gen.Name = GenerateUniqueName(gen.Name);

                foreach (ParameterDefinition parameter in field.Parameters)
                    parameter.Name = GenerateUniqueName(parameter.Name);
            }

            foreach (PropertyDefinition field in type.Properties)
            {
                if (field.IsSpecialName || field.DeclaringType != type)
                    continue;

                field.Name = GenerateUniqueName(field.Name);

                if (field.GetMethod != null && field.GetMethod.DeclaringType == type)
                    field.GetMethod.Name = GenerateUniqueName(field.GetMethod.Name);

                if (field.SetMethod != null && field.SetMethod.DeclaringType == type)
                    field.SetMethod.Name = GenerateUniqueName(field.SetMethod.Name);
            }

            foreach (FieldDefinition field in type.Fields)
                if (!field.HasCustomAttributes && field.DeclaringType == type)
                    field.Name = GenerateUniqueName(field.Name);
        }
    }*/
    internal static void RenameAssembly(AssemblyDefinition assembly)
    {
        ClearMetadata(assembly);

        foreach (var module in assembly.Modules)
            foreach (var type in module.Types)
                if (!string.IsNullOrEmpty(type.Namespace))
                    type.Namespace = GenerateUniqueName(type.Namespace);
        

        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            if ((type.Name.Equals("AppStart") || type.Name.Equals("Client")) && type.IsPublic)
                type.Name = GenerateUniqueName(type.Name);

            if (type.IsEnum || type.IsPublic)
                continue;

            type.Name = GenerateUniqueName(type.Name);

            foreach (MethodDefinition method in type.Methods)
            {
                if (!method.HasBody || method.IsVirtual || method.DeclaringType != type)
                    continue;

                if (!method.IsConstructor && !method.IsSpecialName)
                    method.Name = GenerateUniqueName(method.Name);

                foreach (var gen in method.GenericParameters)
                    gen.Name = GenerateUniqueName(gen.Name);

                foreach (ParameterDefinition parameter in method.Parameters)
                    parameter.Name = GenerateUniqueName(parameter.Name);
            }

            foreach (PropertyDefinition property in type.Properties)
            {
                if (property.IsSpecialName || property.DeclaringType != type)
                    continue;

                property.Name = GenerateUniqueName(property.Name);

                if (property.GetMethod != null && property.GetMethod.DeclaringType == type)
                    property.GetMethod.Name = GenerateUniqueName(property.GetMethod.Name);

                if (property.SetMethod != null && property.SetMethod.DeclaringType == type)
                    property.SetMethod.Name = GenerateUniqueName(property.SetMethod.Name);
            }

            foreach (FieldDefinition field in type.Fields)
                if (!field.HasCustomAttributes && field.DeclaringType == type)
                    field.Name = GenerateUniqueName(field.Name);
        }
        UpdateReferences(assembly);
        ValidateChanges(assembly);
    }
    private static void ClearMetadata(AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)
        {
            module.CustomAttributes.Clear();
            foreach (var type in module.Types)
            {
                type.CustomAttributes.Clear();
                foreach (var method in type.Methods)
                    method.CustomAttributes.Clear();
            }
        }
    }
    private static void UpdateReferences(AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)        
            foreach (var type in module.Types)            
                foreach (var method in type.Methods)                
                    foreach (var instruction in method.Body.Instructions)                    
                        if (instruction.Operand is MethodReference methodRef)
                            methodRef.Name = GenerateUniqueName(methodRef.Name);
                        else if (instruction.Operand is FieldReference fieldRef)                        
                            fieldRef.Name = GenerateUniqueName(fieldRef.Name);                        
                        else if (instruction.Operand is TypeReference typeRef)                        
                            typeRef.Name = GenerateUniqueName(typeRef.Name); 
    }
    private static void ValidateChanges(AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)
        {
            foreach (var type in module.Types)
            {
                if (string.IsNullOrEmpty(type.Name) || string.IsNullOrEmpty(type.Namespace))
                    throw new InvalidOperationException("Type originalName or namespace cannot be empty.");

                foreach (var method in type.Methods)
                    if (string.IsNullOrEmpty(method.Name))
                        throw new InvalidOperationException("Method originalName cannot be empty.");
                    
                foreach (var property in type.Properties)
                    if (string.IsNullOrEmpty(property.Name))
                        throw new InvalidOperationException("Property originalName cannot be empty.");
                    
                foreach (var field in type.Fields)
                    if (string.IsNullOrEmpty(field.Name))
                        throw new InvalidOperationException("Field originalName cannot be empty.");
            }
        }
    }

    static Dictionary<string, string> nameMap = new Dictionary<string, string>();
    public static string GenerateUniqueName() => GenerateUniqueName("Unnamed");
    private static string GenerateUniqueName(string originalName)
    {
        string newName = Guid.NewGuid().ToString("N");
        if (string.IsNullOrEmpty(originalName))
            originalName = newName;

        nameMap[originalName] = newName;
        Console.WriteLine($"Renaming {originalName} to {newName}...");
        return newName;
    }
    public static string GenerateUniqueName<T>(this T member) where T : MemberReference, IMemberDefinition
    {
        string newName = Guid.NewGuid().ToString("N");

        if (string.IsNullOrEmpty(member.Name))
            member.Name = newName;
        
        nameMap[member.Name] = newName;
        Console.WriteLine($"Renaming {member.Name} to {newName}...");
        return newName;
    }
    public static void LogChanges()
    {
        foreach (var entry in nameMap)
            Console.WriteLine($"Original Name: {entry.Key}, New Name: {entry.Value}");
    }

    public static string GetRealisticName() => methodNames[random.Next(methodNames.Length)]; 
    static readonly string[] methodNames =
    [
        "OnLoaded",
        "OnceLoaded",
        "AfterInit",
        "OnFixedUpdate",
        "WaitForMenuMade",
        "StartProcessing",
        "HandleRequest",
        "UpdateStats",
        "InitializeComponents",
        "LoadResources",
        "SaveData",
        "FetchResults",
        "RenderUI",
        "ValidateInput",
        "FixDropDown",
        "OnPlayerIn",
        "OnPlayerLeft",
        "AfterPlayerExists",
        "FindPatch",
        "PreloadAssets",
        "CheckForUpdates",
        "ApplySettings",
        "ResetGame",
        "LoadScene",
        "UnloadScene",
        "PauseGame",
        "ResumeGame",
        "HandleCollision",
        "SpawnEnemy",
        "DestroyObject",
        "PlaySound",
        "StopSound",
        "UpdateScore",
        "DisplayMessage",
        "HideMessage",
        "ActivatePowerUp",
        "DeactivatePowerUp",
        "TrackPlayer",
        "LogEvent",
        "SyncData",
        "GenerateReport",
        "OptimizePerformance",
        "HandleInput",
        "ProcessCommand",
        "UpdateLeaderboard",
        "SaveProgress",
        "LoadProgress",
        "CheckAchievements",
        "UnlockAchievement",
        "HandleError",
        "RetryConnection",
        "UpdateUI",
        "RefreshContent",
        "InitializeGame",
        "TerminateGame",
        "LoadTextures",
        "UnloadTextures",
        "UpdatePhysics",
        "HandleAnimation",
        "StartCutscene",
        "EndCutscene",
        "UpdateInventory",
        "EquipItem",
        "UnequipItem",
        "CraftItem",
        "UseItem",
        "DropItem",
        "PickupItem",
        "OpenChest",
        "CloseChest",
        "StartQuest",
        "CompleteQuest",
        "FailQuest",
        "UpdateQuestLog",
        "ShowMap",
        "HideMap",
        "ZoomIn",
        "ZoomOut",
        "RotateCamera",
        "MoveCamera",
        "LockTarget",
        "UnlockTarget",
        "CastSpell",
        "CancelSpell",
        "UpdateHealth",
        "UpdateMana",
        "UpdateStamina",
        "RegenerateHealth",
        "RegenerateMana",
        "RegenerateStamina",
        "TakeDamage",
        "HealDamage",
        "ApplyBuff",
        "RemoveBuff",
        "ApplyDebuff",
        "RemoveDebuff",
        "UpdateStatusEffect",
        "HandleDeath",
        "RespawnPlayer",
        "SaveCheckpoint",
        "LoadCheckpoint",
        "UpdateLighting",
        "AdjustBrightness",
        "AdjustContrast",
        "UpdateShadows",
        "UpdateReflections",
        "UpdatePostProcessing",
        "HandleNetworkEvent",
        "SendPacket",
        "ReceivePacket",
        "ConnectToServer",
        "DisconnectFromServer",
        "UpdateLatency",
        "SyncTime",
        "UpdateWeather",
        "ChangeSeason",
        "UpdateDayNightCycle",
        "HandleDialogue",
        "StartConversation",
        "EndConversation",
        "UpdateNPC",
        "MoveNPC",
        "AnimateNPC",
        "UpdateAI",
        "HandlePathfinding",
        "UpdateWaypoints",
        "HandleTrigger",
        "ActivateTrigger",
        "DeactivateTrigger",
        "UpdateParticles",
        "SpawnParticles",
        "DestroyParticles",
        "UpdateWater",
        "UpdateTerrain",
        "GenerateTerrain",
        "UpdateVegetation",
        "SpawnVegetation",
        "DestroyVegetation",
        "UpdateBuildings",
        "ConstructBuilding",
        "DemolishBuilding",
        "UpdateVehicles",
        "SpawnVehicle",
        "DestroyVehicle",
        "UpdateTraffic",
        "HandlePedestrian",
        "UpdateEconomy",
        "AdjustPrices",
        "UpdateMarket",
        "HandleTrade",
        "UpdateResources",
        "HarvestResource",
        "ProcessResource",
        "StoreResource",
        "DistributeResource",
        "UpdatePopulation",
        "HandleMigration",
        "UpdateDemographics",
        "HandleDisaster",
        "UpdateEmergencyServices",
        "HandleCrime",
        "UpdateLawEnforcement",
        "HandleFire",
        "UpdateFireDepartment",
        "HandleMedical",
        "UpdateHealthcare",
        "HandleEducation",
        "UpdateSchools",
        "HandleUtilities",
        "UpdatePowerGrid",
        "UpdateWaterSupply",
        "UpdateSewage",
        "UpdateWasteManagement",
        "HandleTransportation",
        "UpdatePublicTransport",
        "HandleTourism",
        "UpdateAttractions",
        "HandleEvents",
        "UpdateCalendar",
        "HandleSports",
        "UpdateTeams",
        "HandleMusic",
        "UpdatePlaylist",
        "HandleArt",
        "UpdateGallery",
        "HandleScience",
        "UpdateResearch",
        "HandleTechnology",
        "UpdateDevices",
        "HandleFashion",
        "UpdateTrends",
        "HandleFood",
        "UpdateRecipes",
        "HandleBeverages",
        "UpdateDrinks",
        "HandleFitness",
        "UpdateWorkouts",
        "HandleWellness",
        "UpdateMeditation",
        "HandleTravel",
        "UpdateDestinations",
        "HandleShopping",
        "UpdateStores",
        "HandleFinance",
        "UpdateAccounts",
        "HandleRealEstate",
        "UpdateProperties",
        "HandleLegal",
        "UpdateLaws",
        "HandlePolitics",
        "UpdatePolicies",
        "HandleReligion",
        "UpdateFaith",
        "HandlePhilosophy",
        "UpdateBeliefs",
        "HandleHistory",
        "UpdateTimeline",
        "HandleCulture",
        "UpdateTraditions",
        "HandleLanguage",
        "UpdateVocabulary",
        "HandleLiterature",
        "UpdateLibrary",
        "HandleCinema",
        "UpdateMovies",
        "HandleTheater",
        "UpdatePlays",
        "HandleDance",
        "UpdateChoreography",
        "HandlePhotography",
        "UpdateAlbums",
        "HandleJournalism",
        "UpdateArticles",
        "HandleBroadcasting",
        "UpdateChannels",
        "HandlePublishing",
        "UpdateBooks",
        "HandleGaming",
        "UpdateScores",
        "HandleEsports",
        "UpdateTournaments",
        "HandleStreaming",
        "UpdateStreams",
        "HandleSocialMedia",
        "UpdatePosts",
        "HandleMessaging",
        "UpdateChats",
        "HandleDating",
        "UpdateMatches",
        "HandleFamily",
        "UpdateRelations",
        "HandleFriendship",
        "UpdateConnections",
        "HandlePets",
        "UpdateAnimals",
        "HandleGardening",
        "UpdatePlants",
        "HandleCooking",
        "UpdateMeals",
        "HandleCleaning",
        "UpdateTasks",
        "HandleMaintenance",
        "UpdateRepairs",
        "HandleConstruction",
        "UpdateProjects",
        "HandleManufacturing",
        "UpdateProduction",
        "HandleLogistics",
        "UpdateShipments",
        "HandleWarehousing",
        "UpdateInventory",
        "HandleRetail",
        "UpdateSales",
        "HandleWholesale",
        "UpdateOrders",
        "HandleCustomerService",
        "UpdateSupport",
        "HandleHumanResources",
        "UpdateEmployees",
        "HandleRecruitment",
        "UpdateCandidates",
        "HandleTraining",
        "UpdateCourses",
        "HandlePerformance",
        "UpdateReviews",
        "HandleCompensation",
        "UpdateSalaries",
        "HandleBenefits",
        "UpdatePlans",
        "HandleCompliance",
        "UpdateRegulations",
        "HandleRiskManagement",
        "UpdateAssessments",
        "HandleSecurity",
        "UpdateProtocols",
        "HandlePrivacy",
        "UpdatePolicies",
        "HandleEthics",
        "UpdateStandards",
        "HandleSustainability",
        "UpdatePractices",
        "HandleInnovation",
        "UpdateIdeas",
        "HandleStrategy",
        "UpdatePlans",
        "HandleLeadership",
        "UpdateVision",
        "HandleManagement",
        "UpdateOperations",
        "HandleMarketing",
        "UpdateCampaigns",
        "HandleSales",
        "UpdateTargets",
        "HandleFinance",
        "UpdateBudgets",
        "HandleAccounting",
        "UpdateLedgers",
        "HandleInvestments",
        "UpdatePortfolio",
        "HandleTaxes",
        "UpdateReturns",
        "HandleAuditing",
        "UpdateReports",
        "HandleLegal",
        "UpdateContracts",
        "HandleIntellectualProperty",
        "UpdatePatents",
        "HandleLitigation",
        "UpdateCases",
        "HandleRegulatory",
        "UpdateCompliance",
        "HandlePublicRelations",
        "UpdatePressReleases",
        "HandleCorporateSocialResponsibility",
        "UpdateInitiatives",
        "HandleGovernmentRelations",
        "UpdateLobbying",
        "HandleCommunityRelations",
        "UpdateEngagement",
        "HandleInvestorRelations",
        "UpdateCommunications",
        "HandleStakeholderRelations",
        "UpdateFeedback",
        "HandleCrisisManagement",
        "UpdatePlans",
        "HandleChangeManagement",
        "UpdateStrategies",
        "HandleProjectManagement",
        "UpdateSchedules",
        "HandleProgramManagement",
        "UpdatePortfolios",
        "HandleProductManagement",
        "UpdateRoadmaps",
        "HandleServiceManagement",
        "UpdateCatalogs",
        "HandleVendorManagement",
        "UpdateContracts",
        "HandleSupplyChainManagement",
        "UpdateLogistics",
        "HandleProcurement",
        "UpdateOrders",
        "HandleInventoryManagement",
        "UpdateStock",
        "HandleQualityManagement",
        "UpdateStandards",
        "HandleEnvironmentalManagement",
        "UpdatePractices",
        "HandleHealthAndSafety",
        "UpdateProtocols",
        "HandleFacilityManagement",
        "UpdateMaintenance",
        "HandleFleetManagement",
        "UpdateVehicles"
    ];
}

