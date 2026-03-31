package com.phimas.bhwmobile

import com.google.gson.Gson
import com.phimas.bhwmobile.model.BhwProfileDto
import com.phimas.bhwmobile.model.DashboardSummaryDto
import com.phimas.bhwmobile.model.TaskItemDto
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Test

class ModelStabilityTest {
    private val gson = Gson()

    @Test
    fun bhwProfileDtoDefaults() {
        val json = "{}"
        val model = gson.fromJson(json, BhwProfileDto::class.java)
        assertNotNull(model)
        assertEquals(0, model.userId)
    }

    @Test
    fun dashboardSummaryDtoDefaults() {
        val json = "{\"bhwName\": \"Test User\"}"
        val model = gson.fromJson(json, DashboardSummaryDto::class.java)
        assertNotNull(model)
        assertEquals("Test User", model.bhwName)
        assertEquals(0, model.assignedTasks)
    }
}
