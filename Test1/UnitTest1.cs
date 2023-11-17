using BirthdayCollector.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using BirthdayCollector.Controllers;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.ComponentModel;

namespace Test1
{
    public class UnitTest1
    {
        private BirthdayCollectorController controller;
      
        [Fact]
        public void Test1()
        {
            // Set up Prerequisites   
            // BirthdayCollectorController controller = new BirthdayCollectorController();
            // Act on Test  
            var notFoundResult = controller.Get("") ;
            // Assert
            Assert.IsType<NotFoundResult>(notFoundResult);
            // Act
            
            // Assert
           // Assert.IsType<OkObjectResult>(okResult as OkObjectResult);
        }

       
    }
}