package com.phimas.bhwmobile.data

import com.phimas.bhwmobile.model.BhwProfileDto
import com.phimas.bhwmobile.model.ChangePasswordRequest
import com.phimas.bhwmobile.model.DashboardSummaryDto
import com.phimas.bhwmobile.model.HealthRecordDto
import com.phimas.bhwmobile.model.InsightDto
import com.phimas.bhwmobile.model.LoginRequest
import com.phimas.bhwmobile.model.MutationResponse
import com.phimas.bhwmobile.model.PatientDirectoryDto
import com.phimas.bhwmobile.model.RecentReportDto
import com.phimas.bhwmobile.model.SubmitConsultationRequest
import com.phimas.bhwmobile.model.SubmitHealthRecordRequest
import com.phimas.bhwmobile.model.TaskItemDto
import com.phimas.bhwmobile.model.UpdateAvailabilityRequest
import com.phimas.bhwmobile.model.UpdateProfileRequest
import com.phimas.bhwmobile.model.UpdateTaskStatusRequest
import com.phimas.bhwmobile.model.UploadProfilePictureRequest
import com.phimas.bhwmobile.model.UserSession
import kotlinx.coroutines.async
import kotlinx.coroutines.coroutineScope

data class BootstrapData(
    val profile: BhwProfileDto,
    val dashboard: DashboardSummaryDto,
    val tasks: List<TaskItemDto>,
    val patients: List<PatientDirectoryDto>,
    val healthRecords: List<HealthRecordDto>,
    val recentReports: List<RecentReportDto>,
    val consultationLogs: List<com.phimas.bhwmobile.model.ConsultationLogDto>,
    val insights: List<InsightDto>,
)

class BhwRepository(
    defaultBaseUrl: String,
    private val sessionStore: SessionStore,
) {
    private var apiBaseUrl: String = normalizeApiBaseUrl(
        sessionStore.loadApiBaseUrl() ?: defaultBaseUrl,
    )
    private var apiService: ApiService = ApiService.create(apiBaseUrl)

    fun getSavedSession(): UserSession? = sessionStore.load()

    fun getApiBaseUrl(): String = apiBaseUrl

    fun updateApiBaseUrl(baseUrl: String): String {
        apiBaseUrl = normalizeApiBaseUrl(baseUrl)
        sessionStore.saveApiBaseUrl(apiBaseUrl)
        apiService = ApiService.create(apiBaseUrl)
        return apiBaseUrl
    }

    fun clearSession() {
        sessionStore.clearSession()
    }

    suspend fun login(username: String, password: String): UserSession {
        val profile = apiService.login(LoginRequest(username = username.trim(), password = password))
        if (profile.success != true) {
            error("Login failed.")
        }

        sessionStore.save(profile)
        return UserSession(userId = profile.userId, username = profile.username)
    }

    suspend fun loadBootstrap(userId: Int): BootstrapData = coroutineScope {
        val profileDeferred = async { getProfile(userId) }
        val dashboardDeferred = async { apiService.getDashboard(userId) }
        val tasksDeferred = async { apiService.getTasks(userId) }
        val patientsDeferred = async { apiService.getPatients(userId) }
        val healthRecordsDeferred = async { apiService.getHealthRecords(userId) }
        val recentReportsDeferred = async { apiService.getRecentReports(userId) }
        val consultationLogsDeferred = async { apiService.getConsultationLogs(userId) }
        val insightsDeferred = async { apiService.getInsights(userId) }

        BootstrapData(
            profile = profileDeferred.await(),
            dashboard = dashboardDeferred.await(),
            tasks = tasksDeferred.await(),
            patients = patientsDeferred.await(),
            healthRecords = healthRecordsDeferred.await(),
            recentReports = recentReportsDeferred.await(),
            consultationLogs = consultationLogsDeferred.await().data,
            insights = insightsDeferred.await(),
        )
    }

    suspend fun getProfile(userId: Int): BhwProfileDto {
        val profile = apiService.getProfile(userId)
        sessionStore.save(profile)
        return profile
    }

    suspend fun updateTaskStatus(taskId: Int, status: String): MutationResponse {
        return apiService.updateTaskStatus(UpdateTaskStatusRequest(taskId = taskId, status = status))
    }

    suspend fun updateProfile(
        userId: Int,
        fullName: String,
        email: String,
        contactNumber: String,
        assignedArea: String,
    ): MutationResponse {
        val response = apiService.updateProfile(
            UpdateProfileRequest(
                userId = userId,
                fullName = fullName,
                email = email,
                contactNumber = contactNumber,
                assignedArea = assignedArea,
            ),
        )
        response.profile?.let(sessionStore::save)
        return response
    }

    suspend fun changePassword(
        userId: Int,
        currentPassword: String,
        newPassword: String,
    ): MutationResponse {
        return apiService.changePassword(
            ChangePasswordRequest(
                userId = userId,
                oldPassword = currentPassword,
                newPassword = newPassword,
            ),
        )
    }

    suspend fun updateAvailability(userId: Int, isAvailable: Boolean): MutationResponse {
        return apiService.updateAvailability(UpdateAvailabilityRequest(userId = userId, isAvailable = isAvailable))
    }

    suspend fun submitHealthRecord(
        userId: Int,
        patientName: String,
        contactNumber: String,
        dateRecorded: String,
        disease: String,
        symptoms: String,
        status: String,
        address: String,
        emergencyContactName: String,
        emergencyContactNumber: String,
    ): MutationResponse {
        return apiService.submitHealthRecord(
            SubmitHealthRecordRequest(
                userId = userId,
                patientName = patientName,
                contactNumber = contactNumber,
                dateRecorded = dateRecorded,
                disease = disease,
                symptoms = symptoms,
                status = status,
                address = address,
                emergencyContactName = emergencyContactName,
                emergencyContactNumber = emergencyContactNumber,
            ),
        )
    }

    suspend fun submitConsultation(
        userId: Int,
        patientName: String,
        contactNumber: String,
        reportType: String,
        content: String,
        dateGenerated: String,
        address: String,
        emergencyContactName: String,
        emergencyContactNumber: String,
    ): MutationResponse {
        return apiService.submitConsultation(
            SubmitConsultationRequest(
                userId = userId,
                patientName = patientName,
                contactNumber = contactNumber,
                dateGenerated = dateGenerated,
                address = address,
                emergencyContactName = emergencyContactName,
                emergencyContactNumber = emergencyContactNumber,
                reportType = reportType,
                content = content,
            ),
        )
    }

    suspend fun uploadProfilePicture(userId: Int, imagePayload: String): MutationResponse {
        val response = apiService.uploadProfilePicture(
            UploadProfilePictureRequest(
                userId = userId,
                image = imagePayload,
            ),
        )
        response.profile?.let(sessionStore::save)
        return response
    }

    private fun normalizeApiBaseUrl(baseUrl: String): String {
        val trimmed = baseUrl.trim()
        require(trimmed.isNotBlank()) { "Server URL is required." }

        val normalized = if (trimmed.startsWith("http://") || trimmed.startsWith("https://")) {
            trimmed
        } else if (isLikelyLocalHost(trimmed)) {
            "http://$trimmed"
        } else {
            "https://$trimmed"
        }

        return if (normalized.endsWith("/")) normalized else "$normalized/"
    }

    private fun isLikelyLocalHost(baseUrl: String): Boolean {
        val normalized = baseUrl.lowercase()
        return normalized.startsWith("localhost") ||
            normalized.startsWith("127.0.0.1") ||
            normalized.startsWith("10.") ||
            normalized.startsWith("192.168.") ||
            Regex("^172\\.(1[6-9]|2\\d|3[0-1])\\.").containsMatchIn(normalized)
    }
}
