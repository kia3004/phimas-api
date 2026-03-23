package com.phimas.bhwmobile.model

import com.google.gson.annotations.SerializedName

data class UserSession(
    val userId: Int,
    val username: String,
)

data class BhwProfileDto(
    @SerializedName("success") val success: Boolean? = null,
    @SerializedName("userId") val userId: Int,
    @SerializedName("username") val username: String,
    @SerializedName("bhwName") val bhwName: String,
    @SerializedName("email") val email: String,
    @SerializedName("contactNumber") val contactNumber: String? = null,
    @SerializedName("assignedArea") val assignedArea: String? = null,
    @SerializedName("isAvailable") val isAvailable: Boolean,
    @SerializedName("profilePicture") val profilePicture: String? = null,
)

data class DashboardSummaryDto(
    @SerializedName("assignedTasks") val assignedTasks: Int,
    @SerializedName("priorityTasks") val priorityTasks: Int,
    @SerializedName("dueTasks") val dueTasks: Int,
    @SerializedName("completedTasks") val completedTasks: Int,
    @SerializedName("progress") val progress: Int,
    @SerializedName("bhwName") val bhwName: String,
    @SerializedName("isAvailable") val isAvailable: Boolean,
)

data class TaskItemDto(
    @SerializedName("id") val id: Int,
    @SerializedName("title") val title: String? = null,
    @SerializedName("description") val description: String? = null,
    @SerializedName("priority") val priority: String? = null,
    @SerializedName("status") val status: String? = null,
    @SerializedName("taskDate") val taskDate: String? = null,
    @SerializedName("householdId") val householdId: Int? = null,
    @SerializedName("householdName") val householdName: String? = null,
    @SerializedName("householdAddress") val householdAddress: String? = null,
)

data class PatientDirectoryDto(
    @SerializedName("patientId") val patientId: Int,
    @SerializedName("patientName") val patientName: String,
    @SerializedName("householdId") val householdId: Int,
    @SerializedName("householdName") val householdName: String? = null,
    @SerializedName("householdAddress") val householdAddress: String? = null,
    @SerializedName("emergencyContactName") val emergencyContactName: String? = null,
    @SerializedName("emergencyContactNumber") val emergencyContactNumber: String? = null,
    @SerializedName("searchText") val searchText: String? = null,
)

data class HealthRecordDto(
    @SerializedName(value = "recordId", alternate = ["id"]) val recordId: Int,
    @SerializedName("householdId") val householdId: Int? = null,
    @SerializedName("householdName") val householdName: String? = null,
    @SerializedName("householdAddress") val householdAddress: String? = null,
    @SerializedName("address") val address: String? = null,
    @SerializedName("visitAddress") val visitAddress: String? = null,
    @SerializedName("patientId") val patientId: Int? = null,
    @SerializedName("patientName") val patientName: String? = null,
    @SerializedName("emergencyContactName") val emergencyContactName: String? = null,
    @SerializedName("emergencyContactNumber") val emergencyContactNumber: String? = null,
    @SerializedName("dateRecorded") val dateRecorded: String? = null,
    @SerializedName("disease") val disease: String? = null,
    @SerializedName("symptoms") val symptoms: String? = null,
    @SerializedName("status") val status: String? = null,
)

data class RecentReportDto(
    @SerializedName(value = "reportId", alternate = ["id"]) val reportId: Int,
    @SerializedName("householdId") val householdId: Int? = null,
    @SerializedName("householdName") val householdName: String? = null,
    @SerializedName("householdAddress") val householdAddress: String? = null,
    @SerializedName("address") val address: String? = null,
    @SerializedName("visitAddress") val visitAddress: String? = null,
    @SerializedName("patientId") val patientId: Int? = null,
    @SerializedName("patientName") val patientName: String? = null,
    @SerializedName("emergencyContactName") val emergencyContactName: String? = null,
    @SerializedName("emergencyContactNumber") val emergencyContactNumber: String? = null,
    @SerializedName("reportType") val reportType: String? = null,
    @SerializedName("content") val content: String? = null,
    @SerializedName("dateGenerated") val dateGenerated: String? = null,
)

data class ConsultationLogDto(
    @SerializedName(value = "reportId", alternate = ["id"]) val reportId: Int,
    @SerializedName("householdId") val householdId: Int? = null,
    @SerializedName("householdName") val householdName: String? = null,
    @SerializedName("householdAddress") val householdAddress: String? = null,
    @SerializedName("address") val address: String? = null,
    @SerializedName("visitAddress") val visitAddress: String? = null,
    @SerializedName("patientId") val patientId: Int? = null,
    @SerializedName("patientName") val patientName: String? = null,
    @SerializedName("emergencyContactName") val emergencyContactName: String? = null,
    @SerializedName("emergencyContactNumber") val emergencyContactNumber: String? = null,
    @SerializedName("reportType") val reportType: String? = null,
    @SerializedName("content") val content: String? = null,
    @SerializedName("dateSubmitted") val dateSubmitted: String? = null,
)

data class ConsultationLogsEnvelope(
    @SerializedName("data") val data: List<ConsultationLogDto> = emptyList(),
)

data class InsightDto(
    @SerializedName("id") val id: Int,
    @SerializedName("title") val title: String? = null,
    @SerializedName("description") val description: String? = null,
    @SerializedName("severity") val severity: String? = null,
)

data class MutationResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("message") val message: String,
    @SerializedName("recordId") val recordId: Int? = null,
    @SerializedName("reportId") val reportId: Int? = null,
    @SerializedName("profilePicture") val profilePicture: String? = null,
    @SerializedName("profile") val profile: BhwProfileDto? = null,
)

data class LoginRequest(
    @SerializedName("username") val username: String,
    @SerializedName("password") val password: String,
)

data class UpdateTaskStatusRequest(
    @SerializedName("taskId") val taskId: Int,
    @SerializedName("id") val id: Int = 0,
    @SerializedName("status") val status: String,
)

data class UpdateProfileRequest(
    @SerializedName("userId") val userId: Int,
    @SerializedName("fullName") val fullName: String,
    @SerializedName("email") val email: String,
    @SerializedName("contactNumber") val contactNumber: String,
    @SerializedName("assignedArea") val assignedArea: String,
)

data class ChangePasswordRequest(
    @SerializedName("userId") val userId: Int,
    @SerializedName("oldPassword") val oldPassword: String,
    @SerializedName("newPassword") val newPassword: String,
)

data class UpdateAvailabilityRequest(
    @SerializedName("userId") val userId: Int,
    @SerializedName("isAvailable") val isAvailable: Boolean,
)

data class SubmitHealthRecordRequest(
    @SerializedName("userId") val userId: Int,
    @SerializedName("patientName") val patientName: String,
    @SerializedName("contactNumber") val contactNumber: String,
    @SerializedName("dateRecorded") val dateRecorded: String,
    @SerializedName("disease") val disease: String,
    @SerializedName("symptoms") val symptoms: String,
    @SerializedName("status") val status: String,
    @SerializedName("address") val address: String,
    @SerializedName("emergencyContactName") val emergencyContactName: String,
    @SerializedName("emergencyContactNumber") val emergencyContactNumber: String,
)

data class SubmitConsultationRequest(
    @SerializedName("userId") val userId: Int,
    @SerializedName("patientName") val patientName: String,
    @SerializedName("contactNumber") val contactNumber: String,
    @SerializedName("dateGenerated") val dateGenerated: String,
    @SerializedName("address") val address: String,
    @SerializedName("emergencyContactName") val emergencyContactName: String,
    @SerializedName("emergencyContactNumber") val emergencyContactNumber: String,
    @SerializedName("reportType") val reportType: String,
    @SerializedName("content") val content: String,
)

data class UploadProfilePictureRequest(
    @SerializedName("userId") val userId: Int,
    @SerializedName("image") val image: String,
)
