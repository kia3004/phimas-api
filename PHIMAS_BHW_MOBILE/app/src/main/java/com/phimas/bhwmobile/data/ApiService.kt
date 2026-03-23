package com.phimas.bhwmobile.data

import com.phimas.bhwmobile.model.BhwProfileDto
import com.phimas.bhwmobile.model.ChangePasswordRequest
import com.phimas.bhwmobile.model.ConsultationLogsEnvelope
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
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Query
import java.util.concurrent.TimeUnit

interface ApiService {
    @POST("api/BHWApi/login")
    suspend fun login(@Body request: LoginRequest): BhwProfileDto

    @GET("api/BHWApi/profile")
    suspend fun getProfile(@Query("userId") userId: Int): BhwProfileDto

    @GET("api/BHWApi/dashboard")
    suspend fun getDashboard(@Query("userId") userId: Int): DashboardSummaryDto

    @GET("api/BHWApi/tasks")
    suspend fun getTasks(@Query("userId") userId: Int): List<TaskItemDto>

    @POST("api/BHWApi/updateTaskStatus")
    suspend fun updateTaskStatus(@Body request: UpdateTaskStatusRequest): MutationResponse

    @GET("api/BHWApi/patients")
    suspend fun getPatients(@Query("userId") userId: Int): List<PatientDirectoryDto>

    @GET("api/BHWApi/healthrecords")
    suspend fun getHealthRecords(@Query("userId") userId: Int): List<HealthRecordDto>

    @POST("api/BHWApi/healthrecords")
    suspend fun submitHealthRecord(@Body request: SubmitHealthRecordRequest): MutationResponse

    @GET("api/BHWApi/reports/recent")
    suspend fun getRecentReports(@Query("userId") userId: Int): List<RecentReportDto>

    @GET("api/BHWApi/consultationlogs")
    suspend fun getConsultationLogs(@Query("userId") userId: Int): ConsultationLogsEnvelope

    @POST("api/BHWApi/submitConsultation")
    suspend fun submitConsultation(@Body request: SubmitConsultationRequest): MutationResponse

    @POST("api/BHWApi/updateProfile")
    suspend fun updateProfile(@Body request: UpdateProfileRequest): MutationResponse

    @POST("api/BHWApi/changePassword")
    suspend fun changePassword(@Body request: ChangePasswordRequest): MutationResponse

    @POST("api/BHWApi/availability")
    suspend fun updateAvailability(@Body request: UpdateAvailabilityRequest): MutationResponse

    @GET("api/BHWApi/insights")
    suspend fun getInsights(@Query("userId") userId: Int): List<InsightDto>

    @POST("api/BHWApi/uploadProfilePic")
    suspend fun uploadProfilePicture(@Body request: UploadProfilePictureRequest): MutationResponse

    companion object {
        fun create(baseUrl: String): ApiService {
            val normalizedBaseUrl = if (baseUrl.endsWith("/")) baseUrl else "$baseUrl/"
            val logger = HttpLoggingInterceptor().apply {
                level = HttpLoggingInterceptor.Level.BASIC
            }
            val client = OkHttpClient.Builder()
                .connectTimeout(30, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .addInterceptor(logger)
                .build()

            return Retrofit.Builder()
                .baseUrl(normalizedBaseUrl)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(ApiService::class.java)
        }
    }
}
