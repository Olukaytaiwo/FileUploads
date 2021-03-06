USE [raregistars-db]
GO
/****** Object:  Table [dbo].[NseProposals]    Script Date: 27/05/2022 09:17:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NseProposals](
	[FirstName] [nvarchar](450) NOT NULL,
	[LastName] [nvarchar](max) NULL,
	[PolicyId] [nvarchar](max) NULL,
	[CompanyName] [nvarchar](max) NULL,
	[CustomerNumber] [nvarchar](max) NULL,
	[ProductName] [nvarchar](max) NULL,
	[SumAssured] [nvarchar](max) NULL,
	[LastPremiumPaid] [nvarchar](max) NULL,
	[CoverStartDate] [datetime2](7) NOT NULL,
	[CoverEndDate] [datetime2](7) NOT NULL,
	[FullName] [nvarchar](max) NULL,
	[UserName] [nvarchar](max) NULL,
	[UserId] [nvarchar](max) NULL,
	[EmailAddress] [nvarchar](max) NULL,
	[AmountPayable] [float] NOT NULL,
	[TelephoneNumber] [nvarchar](max) NULL,
	[RcNumber] [nvarchar](max) NULL,
	[QuoteId] [nvarchar](max) NULL,
	[PricingOption] [nvarchar](max) NULL,
	[Tier] [nvarchar](max) NULL,
	[DateUploaded] [nvarchar](max) NULL,
	[AxaLogo] [varbinary](max) NULL,
 CONSTRAINT [PK_NseProposals] PRIMARY KEY CLUSTERED 
(
	[FirstName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
