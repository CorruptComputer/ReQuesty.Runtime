using System.Globalization;
using System.Text;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Serialization.Text.Tests.Mocks;
using Xunit;

namespace ReQuesty.Runtime.Serialization.Text.Tests
{
    public class TextSerializationWriterTests
    {
        public TextSerializationWriterTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
        }

        [Fact]
        public void WritesStringValue()
        {
            // Arrange
            string value = "This is a string value";

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteStringValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal(value, serializedString);
        }

        [Fact]
        public void StreamIsReadableAfterDispose()
        {
            // Arrange
            string value = "This is a string value";

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteStringValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            // Dispose the writer
            textSerializationWriter.Dispose();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal(value, serializedString);
        }

        [Fact]
        public void WriteBoolValue_IsWrittenCorrectly()
        {
            // Arrange
            bool value = true;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteBoolValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("True", serializedString);
        }

        [Fact]
        public void WriteByteArrayValue_IsWrittenCorrectly()
        {
            // Arrange
            byte[] value = new byte[] { 2, 4, 6 };

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteByteArrayValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("AgQG", serializedString);
        }

        [Fact]
        public void WriteByteValue_IsWrittenCorrectly()
        {
            // Arrange
            byte value = 5;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteByteValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("5", serializedString);
        }

        [Fact]
        public void WriteDateTimeOffsetValue_IsWrittenCorrectly()
        {
            // Arrange
            DateTimeOffset value = new(2024, 11, 30, 15, 35, 45, 987, TimeSpan.FromHours(3));

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteDateTimeOffsetValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("2024-11-30T15:35:45.9870000+03:00", serializedString);
        }

        [Fact]
        public void WriteDateValue_IsWrittenCorrectly()
        {
            // Arrange
            Date value = new(2024, 11, 30);

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteDateValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("2024-11-30", serializedString);
        }

        [Fact]
        public void WriteDecimalValue_IsWrittenCorrectly()
        {
            // Arrange
            decimal value = 36.8m;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteDecimalValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("36.8", serializedString);
        }

        [Fact]
        public void WriteDoubleValue_IsWrittenCorrectly()
        {
            // Arrange
            double value = 36.8d;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteDoubleValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("36.8", serializedString);
        }

        [Fact]
        public void WriteFloatValue_IsWrittenCorrectly()
        {
            // Arrange
            float value = 36.8f;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteFloatValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("36.8", serializedString);
        }

        [Fact]
        public void WriteGuidValue_IsWrittenCorrectly()
        {
            // Arrange
            Guid value = new("3adeb301-58f1-45c5-b820-ae5f4af13c89");

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteGuidValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("3adeb301-58f1-45c5-b820-ae5f4af13c89", serializedString);
        }

        [Fact]
        public void WriteIntegerValue_IsWrittenCorrectly()
        {
            // Arrange
            int value = 25;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteIntValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("25", serializedString);
        }

        [Fact]
        public void WriteLongValue_IsWrittenCorrectly()
        {
            // Arrange
            long value = long.MaxValue;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteLongValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("9223372036854775807", serializedString);
        }

        [Fact]
        public void WriteNullValue_IsWrittenCorrectly()
        {
            // Arrange
            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteNullValue(null);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("null", serializedString);
        }

        [Fact]
        public void WriteSByteValue_IsWrittenCorrectly()
        {
            // Arrange
            sbyte value = sbyte.MaxValue;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteSbyteValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("127", serializedString);
        }

        [Fact]
        public void WriteTimeValue_IsWrittenCorrectly()
        {
            // Arrange
            Time value = new(23, 46, 59);

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteTimeValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("23:46:59", serializedString);
        }

        [Fact]
        public void WriteTimeSpanValue_IsWrittenCorrectly()
        {
            // Arrange
            TimeSpan value = new(756, 4, 6, 8, 10);

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteTimeSpanValue(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("P756DT4H6M8.01S", serializedString);
        }

        [Fact]
        public void WriteAdditionalData_ThrowsInvalidOperationException()
        {
            // Arrange
            Dictionary<string, object> additionalData = [];

            using TextSerializationWriter textSerializationWriter = new();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => textSerializationWriter.WriteAdditionalData(additionalData));
        }

        [Fact]
        public void WriteEnumValue_IsWrittenCorrectly()
        {
            // Arrange
            TestEnum value = TestEnum.FirstItem;

            using TextSerializationWriter textSerializationWriter = new();

            // Act
            textSerializationWriter.WriteEnumValue<TestEnum>(null, value);
            Stream contentStream = textSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("FirstItem", serializedString);
        }


        [Fact]
        public void WriteEnumValueWithAttribute_IsWrittenCorrectly()
        {
            // Arrange
            TestNamingEnum value = TestNamingEnum.Item2SubItem1;

            using TextSerializationWriter formSerializationWriter = new();

            // Act
            formSerializationWriter.WriteEnumValue<TestNamingEnum>(null, value);
            Stream contentStream = formSerializationWriter.GetSerializedContent();
            using StreamReader reader = new(contentStream, Encoding.UTF8);
            string serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("Item2:SubItem1", serializedString);
        }

        [Fact]
        public void WriteCollectionOfEnumValues_ThrowsInvalidOperationException()
        {
            // Arrange
            List<TestEnum?> value = [TestEnum.FirstItem, TestEnum.SecondItem];

            using TextSerializationWriter textSerializationWriter = new();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => textSerializationWriter.WriteCollectionOfEnumValues(null, value));
        }
    }
}
